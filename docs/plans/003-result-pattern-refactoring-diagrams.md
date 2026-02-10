# Result Pattern Refactoring - Architecture Diagrams

## Current vs Target Architecture

### Current Flow (Tuple-Based)

```
┌────────────────────────────────────────────────────────────────────┐
│                         HTTP Request                               │
└────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────┐
│  Controller                                                        │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  var (success, data, error) = await _service.DoAsync();    │   │
│  │  if (!success)                                              │   │
│  │      return BadRequest(new { message = error });            │   │
│  │  return Ok(data);                                           │   │
│  └────────────────────────────────────────────────────────────┘   │
│  ❌ Business logic mixed with presentation logic                  │
│  ❌ Inconsistent error handling                                   │
└────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────┐
│  Service                                                           │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  if (validation fails)                                       │   │
│  │      return (false, null, "Error message");                  │   │
│  │  // ... do work ...                                          │   │
│  │  return (true, data, null);                                  │   │
│  └────────────────────────────────────────────────────────────┘   │
│  ⚠️ Tuple destructuring in controller                             │
└────────────────────────────────────────────────────────────────────┘
```

### Target Flow (Result Pattern)

```
┌────────────────────────────────────────────────────────────────────┐
│                         HTTP Request                               │
└────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────┐
│  Controller (Thin)                                                 │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  return await _service.DoAsync()                           │   │
│  │      .Match(                                               │   │
│  │          onSuccess: data => Ok(data),                      │   │
│  │          onFailure: error => BadRequest(new { error })     │   │
│  │      );                                                    │   │
│  └────────────────────────────────────────────────────────────┘   │
│  ✅ Single expression for both paths                              │
│  ✅ Consistent pattern across all endpoints                        │
└────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌────────────────────────────────────────────────────────────────────┐
│  Service                                                           │
│  ┌────────────────────────────────────────────────────────────┐   │
│  │  if (validation fails)                                       │   │
│  │      return Result.Failure<Data>("Error message");           │   │
│  │  // ... do work ...                                          │   │
│  │  return Result.Success(data);                                │   │
│  └────────────────────────────────────────────────────────────┘   │
│  ✅ Explicit success/failure semantics                             │
│  ✅ Functional chaining support                                   │
└────────────────────────────────────────────────────────────────────┘
```

## Layer Changes

### Application Layer (Interfaces)

```diff
// IAuthService.cs
- Task<(bool Success, AuthResponse? Response, string? Error)>
-     LoginAsync(LoginRequest request, CancellationToken cancellationToken);

+ Task<Result<AuthResponse>>
+     LoginAsync(LoginRequest request, CancellationToken cancellationToken);
```

### Infrastructure Layer (Services)

```diff
// AuthService.cs
public async Task<Result<AuthResponse>> LoginAsync(
    LoginRequest request,
    CancellationToken cancellationToken)
{
    var user = await _userManager.FindByEmailAsync(request.Email);

-   if (user == null)
-       return (false, null, "Invalid email or password");

+   if (user == null)
+       return Result.Failure<AuthResponse>(Errors.Auth.InvalidCredentials);

    var isValid = await _userManager.CheckPasswordAsync(user, request.Password);

-   if (!isValid)
-       return (false, null, "Invalid email or password");

+   if (!isValid)
+       return Result.Failure<AuthResponse>(Errors.Auth.InvalidCredentials);

    var token = _tokenService.GenerateAccessToken(user, roles);
    var response = new AuthResponse(token, ...);

-   return (true, response, null);
+   return Result.Success(response);
}
```

### API Layer (Controllers)

```diff
// AuthController.cs
[HttpPost("login")]
public async Task<IActionResult> Login(
    [FromBody] LoginRequest request,
    CancellationToken cancellationToken)
{
-   var (success, response, error) = await _authService.LoginAsync(request, cancellationToken);
-
-   if (!success)
-       return Unauthorized(new { message = error });
-
-   return Ok(response);

+   return await _authService.LoginAsync(request, cancellationToken)
+       .Match(
+           onSuccess: response => Ok(response),
+           onFailure: error => Unauthorized(new { message = error })
+       );
}
```

## Data Flow Comparison

### Error Flow: User Not Found

```
┌───────────────────────┐
│   Controller          │
└───────────┬───────────┘
            │
            │ "GET /albums/123"
            │
            ▼
┌───────────────────────────────────────────────────────────────────┐
│  Current Tuple Pattern                                             │
├───────────────────────────────────────────────────────────────────┤
│  Controller:                                                       │
│    1. var album = await _repo.GetByIdAsync(123);                  │
│    2. if (album == null)                                          │
│           return NotFound();                                       │
│    3. if (album.UserId != GetUserId())                            │
│           return Forbid();                                         │
│    4. return Ok(Map(album));                                       │
│                                                                   │
│  ❌ Logic scattered across controller                             │
│  ❌ No clear separation of concerns                                │
└───────────────────────────────────────────────────────────────────┘

┌───────────────────────────────────────────────────────────────────┐
│  Target Result Pattern                                            │
├───────────────────────────────────────────────────────────────────┤
│  Controller:                                                       │
│    return _albumService.GetAlbumAsync(GetUserId(), 123)           │
│        .ToNotFoundResult("Album not found");                      │
│                                                                   │
│  Service:                                                          │
│    1. var album = await _repo.GetByIdAsync(123);                  │
│    2. if (album == null)                                          │
│           return Result.Failure<AlbumResponse>("Not found");       │
│    3. if (album.UserId != userId)                                 │
│           return Result.Failure<AlbumResponse>("Unauthorized");    │
│    4. return Result.Success(Map(album));                          │
│                                                                   │
│  ✅ All business logic in service                                 │
│  ✅ Controller only handles HTTP mapping                          │
└───────────────────────────────────────────────────────────────────┘
```

## Migration Flow by Phase

### Phase 1: Auth (Foundation)

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Package       │────▶│   Interfaces    │────▶│   Service       │
│   Install       │     │   Update        │     │   Refactor      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                                        │
                                                        ▼
                                                 ┌─────────────────┐
                                                 │   Controller    │
                                                 │   Update        │
                                                 └─────────────────┘
                                                        │
                                                        ▼
                                                 ┌─────────────────┐
                                                 │   Tests         │
                                                 │   Update        │
                                                 └─────────────────┘
```

### Phases 2-5: Feature Services

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Create        │────▶│   Implement     │────▶│   Controller    │
│   Interface     │     │   Service       │     │   Refactor      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
                                                        │
                                                        ▼
                                                 ┌─────────────────┐
                                                 │   Tests         │
                                                 │   Write/Update  │
                                                 └─────────────────┘
```

## Extension Methods Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                     ResultExtensions.cs                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  Result<T>                                                          │
│    ├── .ToActionResult()           ──▶ Ok() or BadRequest()        │
│    ├── .ToNotFoundResult()         ──▶ Ok() or NotFound()          │
│    ├── .ToUnauthorizedResult()     ──▶ Ok() or Unauthorized()      │
│    └── .ToForbidResult()           ──▶ Ok() or Forbid()            │
│                                                                     │
│  Result                                                             │
│    ├── .ToActionResult()           ──▶ Ok() or BadRequest()        │
│    └── .ToNoContentResult()        ──▶ NoContent() or BadRequest() │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────┐
│                     ApiControllerBase.cs                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  protected string GetUserId()                                      │
│  protected IActionResult ToActionResult(Result result)             │
│  protected IActionResult ToActionResult<T>(Result<T> result)       │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Error Handling Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│  Error Source                                                       │
└─────────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Service Layer                                                      │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  return Result.Failure<T>(Errors.Photos.NotFound);          │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                              │                                       │
│                              │ Log warning                           │
│                              ▼                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  _logger.LogWarning("Photo not found: {PhotoId}", id);      │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Controller Layer                                                   │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  return result.ToNotFoundResult("Photo not found");         │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                              │                                       │
│                              ▼                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  HTTP 404                                                   │   │
│  │  { "error": "Photo not found" }                             │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Client                                                             │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │  if (error) show error message                               │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

## Service Layer Responsibility

### Before Refactoring

```
┌──────────────────────────────────────────────────────────────┐
│  PhotosController                                            │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  ❌ File validation logic                              │  │
│  │  ❌ Ownership checks                                   │  │
│  │  ❌ Business rules (e.g., max file size)               │  │
│  │  ❌ Resource not found handling                        │  │
│  │  ❌ Authorization checks                               │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  PhotoRepository (data access only)                    │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

### After Refactoring

```
┌──────────────────────────────────────────────────────────────┐
│  PhotosController (Thin)                                      │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  ✅ HTTP concerns only                                  │  │
│  │  ✅ Status code mapping                                 │  │
│  │  ✅ Request/response DTO mapping                        │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  PhotoService (Business Logic)                         │  │
│  │  ✅ File validation                                     │  │
│  │  ✅ Ownership checks                                    │  │
│  │  ✅ Business rules                                      │  │
│  │  ✅ Returns Result<T>                                   │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  PhotoRepository (data access only)                    │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

## Result Type Hierarchy

```
                 Result
                    │
        ┌───────────┴───────────┐
        │                       │
    Result<T>              Unit Result
        │                       │
    ┌───┴───┐               ┌───┴───┐
    │       │               │       │
 Success  Failure       Success  Failure
    │       │               │       │
  Has     Has           Has     Has
 Value   Error          Data   Error
```

## Test Structure Changes

### Before

```
Tests/
├── AuthServiceTests.cs
│   ├── RegisterAsync_EmailExists_ReturnsFalse
│   ├── RegisterAsync_Success_ReturnsTrue
│   └── LoginAsync_InvalidCredentials_ReturnsFalse
│
└── AlbumsControllerTests.cs
    ├── GetAlbum_NotFound_Returns404
    ├── GetAlbum_Unauthorized_Returns403
    └── GetAlbum_Success_Returns200
```

### After

```
Tests/
├── AuthServiceTests.cs
│   ├── RegisterAsync_EmailExists_ReturnsFailure
│   ├── RegisterAsync_Success_ReturnsSuccess
│   ├── LoginAsync_InvalidCredentials_ReturnsFailure
│   └── ResultExtensions_AreWorking_Correctly
│
├── AlbumServiceTests.cs (New)
│   ├── GetAlbumAsync_NotFound_ReturnsFailure
│   ├── GetAlbumAsync_Unauthorized_ReturnsFailure
│   ├── GetAlbumAsync_Success_ReturnsAlbum
│   └── AddPhotoAsync_AlbumNotFound_ReturnsFailure
│
└── AlbumsControllerTests.cs
    ├── GetAlbum_MapsSuccessTo200
    ├── GetAlbum_MapsNotFoundTo404
    └── GetAlbum_MapsUnauthorizedTo403
```
