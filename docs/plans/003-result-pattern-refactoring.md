# Result Pattern Refactoring Plan

## Overview

Refactor MyPhotoBooth backend to use the Result pattern from CSharpFunctionalExtensions instead of tuple-based error handling and exception throwing for business logic validation.

**Current State:**
- Services return tuples `(bool Success, T? Data, string? Error)`
- Controllers manually check success flag and return appropriate HTTP status codes
- Some business logic validation happens in controllers
- Exceptions thrown for authorization failures (`UnauthorizedAccessException`)

**Target State:**
- Services return `Result<T>` or `Result` types
- Controllers use `.Match()` to handle success/failure paths
- All business validation centralized in services
- Consistent error response format across all endpoints

---

## 1. Package Installation

### Required NuGet Package
```bash
dotnet add src/MyPhotoBooth.Application package CSharpFunctionalExtensions
```

**Why Application layer?**
- Result types will be used in interface signatures (Application layer)
- Domain layer should remain dependency-free
- Infrastructure implements Application interfaces
- API consumes Application interfaces

### Alternative: Custom Result Types

**Consideration:** Create lightweight Result types in Domain layer to avoid external dependency.

**Pros:**
- Zero external dependencies
- Full control over API
- Domain layer independence

**Cons:**
- Maintenance burden
- Less battle-tested
- Missing extension methods

**Recommendation:** Use CSharpFunctionalExtensions for robustness and ecosystem support.

---

## 2. Result Type Strategy

### Type Selection

| Scenario | Type |
|----------|------|
| Operations returning data | `Result<T>` |
| Operations without return value | `Result` |
| Operations with multiple return values | `Result<(T1, T2)>` or custom DTO |
| Validation failures | `Result.Failure(error)` |

### Error Handling Strategy

**Option A: String Errors (Simple)**
```csharp
return Result.Failure<User>("Email already exists");
```

**Option B: Error Enum/Class (Structured)**
```csharp
public enum AuthError
{
    EmailAlreadyExists,
    InvalidCredentials,
    UserNotFound
}
return Result.Failure<User>(AuthError.EmailAlreadyExists);
```

**Recommendation:** Start with string errors for simplicity. Consider custom Error type later if:
- Need for i18n
- Need for error codes
- Need for structured error metadata

### HTTP Status Code Mapping

```csharp
public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        Func<T, IActionResult> onSuccess,
        Func<string, IActionResult>? onFailure = null)
    {
        return result.Match(
            onSuccess,
            onFailure ?? (error => new BadRequestObjectResult(new { error }))
        );
    }

    public static IActionResult ToActionResult(
        this Result result,
        IActionResult onSuccess,
        Func<string, IActionResult>? onFailure = null)
    {
        return result.Match(
            onSuccess,
            onFailure ?? (error => new BadRequestObjectResult(new { error }))
        );
    }

    public static IActionResult ToNotFoundResult<T>(this Result<T> result, string message)
        => result.Match(
            onSuccess: data => new OkObjectResult(data),
            onFailure: error => new NotFoundObjectResult(new { error: message })
        );

    public static IActionResult ToUnauthorizedResult<T>(this Result<T> result, string message)
        => result.Match(
            onSuccess: data => new OkObjectResult(data),
            onFailure: error => new UnauthorizedObjectResult(new { error: message })
        );
}
```

---

## 3. Common Validation Scenarios

### Current Pattern → Result Pattern

| Scenario | Current | Result Pattern |
|----------|---------|----------------|
| Email exists | `return (false, "Email exists")` | `return Result.Failure("Email exists")` |
| User not found | `return (false, null, "Not found")` | `return Result.Failure<User>("Not found")` |
| Invalid credentials | `throw UnauthorizedException` | `return Result.Failure<AuthResponse>("Invalid credentials")` |
| Invalid file | `return BadRequest()` in controller | `return Result.Failure("Invalid file")` |
| Token expired | `return (false, null, "Expired")` | `return Result.Failure("Token expired")` |

### Error Message Constants

Create `Application/Common/Errors.cs`:
```csharp
namespace MyPhotoBooth.Application.Common;

public static class Errors
{
    public static class Auth
    {
        public const string EmailAlreadyExists = "Email already registered";
        public const string InvalidCredentials = "Invalid email or password";
        public const string UserNotFound = "User not found";
        public const string InvalidToken = "Invalid or expired token";
        public const string PasswordResetFailed = "Failed to reset password";
    }

    public static class Photos
    {
        public const string NotFound = "Photo not found";
        public const string UnauthorizedAccess = "You do not have access to this photo";
        public const string InvalidFile = "Invalid image file";
        public const string FileTooLarge = "File size exceeds limit";
        public const string StorageError = "Failed to store file";
    }

    public static class Albums
    {
        public const string NotFound = "Album not found";
        public const string UnauthorizedAccess = "You do not have access to this album";
        public const string PhotoNotInAlbum = "Photo is not in this album";
    }

    public static class Tags
    {
        public const string NotFound = "Tag not found";
        public const string UnauthorizedAccess = "You do not have access to this tag";
    }

    public static class ShareLinks
    {
        public const string NotFound = "Share link not found";
        public const string Expired = "Share link has expired";
        public const string Revoked = "Share link has been revoked";
        public const string InvalidPassword = "Incorrect password";
        public const string DownloadNotAllowed = "Download is not allowed";
    }
}
```

---

## 4. Migration Strategy

### Recommendation: Incremental Migration

**Approach:** Migrate one service/controller pair at a time.

**Rationale:**
- Lower risk - can test each feature independently
- Can ship incrementally
- Easier to review and revert
- Allows learning curve adjustment

**Order of Migration:**

1. **Phase 1: Auth (AuthService + AuthController)**
   - Already uses tuple pattern (closest to Result)
   - Isolated from other services
   - Good test case for pattern

2. **Phase 2: ShareLinks (ShareLinksController + SharedController)**
   - Newer feature, less established
   - Good validation scenarios

3. **Phase 3: Albums (AlbumsController)**
   - CRUD operations
   - Authorization checks

4. **Phase 4: Tags (TagsController)**
   - Simpler CRUD
   - Build on learnings

5. **Phase 5: Photos (PhotosController)**
   - Most complex
   - File operations
   - Last to migrate

### Transition Support

**During transition:** Keep both patterns working.

```csharp
// Helper to bridge between patterns
public static class ResultAdapter
{
    public static Result<T> FromTuple<T>((bool Success, T? Data, string? Error) tuple)
        => tuple.Success
            ? Result.Success(tuple.Data!)
            : Result.Failure<T>(tuple.Error ?? "Unknown error");

    public static Result FromTuple((bool Success, string? Error) tuple)
        => tuple.Success
            ? Result.Success()
            : Result.Failure(tuple.Error ?? "Unknown error");
}
```

---

## 5. Implementation Phases

### Phase 1: Auth Service (v1.2.0)

**Files to modify:**
- `Application/Interfaces/IAuthService.cs`
- `Infrastructure/Identity/AuthService.cs`
- `API/Controllers/AuthController.cs`

**Interface Changes:**
```csharp
// Before
Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

// After
Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

// Before
Task<(bool Success, AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

// After
Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
```

**Controller Changes:**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
{
    var result = await _authService.RegisterAsync(request, cancellationToken);

    return result.Match(
        onSuccess: () => Ok(new { message = "Registration successful" }),
        onFailure: error => BadRequest(new { message = error })
    );
}
```

### Phase 2: ShareLinks Service (v1.2.1)

**New Service to Create:**
- `Application/Interfaces/IShareLinkService.cs`
- `Infrastructure/ShareLinkService.cs`
- Move business logic from ShareLinksController to service

**Rationale:** Current implementation has business logic in controllers. Extract to service first, then apply Result pattern.

### Phase 3: Albums Service (v1.2.2)

**New Service to Create:**
- `Application/Interfaces/IAlbumService.cs`
- `Infrastructure/AlbumService.cs`

**Controller becomes thin:**
```csharp
[HttpPost]
public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumRequest request, CancellationToken cancellationToken)
{
    var result = await _albumService.CreateAlbumAsync(GetUserId(), request, cancellationToken);

    return result.Match(
        onSuccess: album => Ok(new AlbumResponse { ... }),
        onFailure: error => BadRequest(new { message = error })
    );
}
```

### Phase 4: Tags Service (v1.2.3)

**New Service to Create:**
- `Application/Interfaces/ITagService.cs`
- `Infrastructure/TagService.cs`

### Phase 5: Photos Service (v1.2.4)

**New Service to Create:**
- `Application/Interfaces/IPhotoService.cs`
- `Infrastructure/PhotoService.cs`

**Most complex refactor:**
- File upload validation
- Image processing integration
- Storage service coordination

---

## 6. API Response Consistency

### Standard Error Response

```csharp
public class ErrorResponse
{
    public string Error { get; set; }
    public string? Code { get; set; }
    public IDictionary<string, string[]>? ValidationErrors { get; set; }
}
```

### Controller Base Class

Create `API/Controllers/ApiControllerBase.cs`:
```csharp
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        return result.Match(
            onSuccess ?? (data => Ok(data)),
            onFailure: error => BadRequest(new ErrorResponse { Error = error })
        );
    }

    protected IActionResult ToActionResult(Result result, IActionResult? onSuccess = null)
    {
        return result.Match(
            onSuccess ?? Ok(),
            onFailure: error => BadRequest(new ErrorResponse { Error = error })
        );
    }

    protected IActionResult ToNotFoundResult<T>(Result<T> result, string notFoundMessage)
    {
        return result.Match(
            onSuccess: data => Ok(data),
            onFailure: error => NotFound(new ErrorResponse { Error = notFoundMessage })
        );
    }
}
```

---

## 7. Logging Considerations

### Logging Without Exceptions

```csharp
public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed for email: {Email}", request.Email);
            return Result.Failure<AuthResponse>(Errors.Auth.InvalidCredentials);
        }

        var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
        {
            _logger.LogWarning("Login attempt failed for user: {UserId}", user.Id);
            return Result.Failure<AuthResponse>(Errors.Auth.InvalidCredentials);
        }

        _logger.LogInformation("User logged in: {UserId}", user.Id);
        // ... rest of implementation
    }
}
```

### Structured Logging

```csharp
_logger.LogResult(result, "Login completed for {Email}", request.Email);

// Extension method
public static class LoggerExtensions
{
    public static void LogResult<T>(this ILogger logger, Result<T> result, string successMessage, params object[] args)
    {
        if (result.IsSuccess)
        {
            logger.LogInformation(successMessage, args);
        }
        else
        {
            logger.LogWarning("Operation failed: {Error}. {Context}", result.Error, string.Format(successMessage, args));
        }
    }
}
```

---

## 8. Testing Impact

### Unit Test Changes

**Before:**
```csharp
[Fact]
public async Task RegisterAsync_EmailExists_ReturnsFailure()
{
    // Arrange
    _userManager.FindByEmailAsync(default!).ReturnsForAnyArgs(new ApplicationUser());

    // Act
    var (success, error) = await _service.RegisterAsync(_request);

    // Assert
    Assert.False(success);
    Assert.Equal("Email already registered", error);
}
```

**After:**
```csharp
[Fact]
public async Task RegisterAsync_EmailExists_ReturnsFailure()
{
    // Arrange
    _userManager.FindByEmailAsync(default!).ReturnsForAnyArgs(new ApplicationUser());

    // Act
    var result = await _service.RegisterAsync(_request);

    // Assert
    Assert.True(result.IsFailure);
    Assert.Equal("Email already registered", result.Error);
}
```

### Test Helper Extensions

```csharp
public static class ResultAssertions
{
    public static void AssertSuccess<T>(this Result<T> result, T? expectedValue = default)
    {
        Assert.True(result.IsSuccess, $"Expected success but got error: {result.Error}");
        if (expectedValue != null)
            Assert.Equal(expectedValue, result.Value);
    }

    public static void AssertFailure(this Result result, string expectedError)
    {
        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }
}
```

---

## 9. File-by-File Change List

### New Files to Create

| File | Purpose |
|------|---------|
| `Application/Common/Errors.cs` | Centralized error constants |
| `Application/Common/ResultExtensions.cs` | Result → HTTP status mapping |
| `API/Controllers/ApiControllerBase.cs` | Base controller with Result helpers |
| `Application/Interfaces/IShareLinkService.cs` | Extract business logic |
| `Application/Interfaces/IAlbumService.cs` | Extract business logic |
| `Application/Interfaces/ITagService.cs` | Extract business logic |
| `Application/Interfaces/IPhotoService.cs` | Extract business logic |

### Files to Modify

| File | Changes |
|------|---------|
| `Application/MyPhotoBooth.Application.csproj` | Add CSharpFunctionalExtensions |
| `Application/Interfaces/IAuthService.cs` | Update signatures to Result |
| `Infrastructure/Identity/AuthService.cs` | Implement Result pattern |
| `API/Controllers/AuthController.cs` | Use .Match() for Result |
| `API/Controllers/ShareLinksController.cs` | Call service, use Result |
| `API/Controllers/SharedController.cs` | Call service, use Result |
| `Infrastructure/ShareLinkService.cs` | New file, Result pattern |
| `API/Controllers/AlbumsController.cs` | Call service, use Result |
| `Infrastructure/AlbumService.cs` | New file, Result pattern |
| `API/Controllers/TagsController.cs` | Call service, use Result |
| `Infrastructure/TagService.cs` | New file, Result pattern |
| `API/Controllers/PhotosController.cs` | Call service, use Result |
| `Infrastructure/PhotoService.cs` | New file, Result pattern |

---

## 10. Potential Pitfalls

### 1. Over-Abstraction
**Risk:** Creating services that just pass through to repositories.

**Solution:** Only create services when there's business logic to encapsulate.

### 2. Exception Handling Confusion
**Risk:** Mixing Result pattern with exceptions.

**Solution:**
- Use Result for business logic validation
- Keep exceptions for truly exceptional conditions (DB down, network failure)
- Global exception handler still needed for unexpected errors

### 3. Async/Await + Result
**Risk:** Nested `Task<Result<T>>` can be awkward.

**Solution:** Use CSharpFunctionalExtensions async extensions:
```csharp
public async Task<Result<User>> GetUserAsync(string email)
    => (await _userManager.FindByEmailAsync(email))
        .ToResult("User not found");
```

### 4. Nullable Ambiguity
**Risk:** `Result<User?>` vs `Result<User>` for not-found scenarios.

**Solution:** Be consistent:
- Use `Result<T>` where T is the found entity
- Return `Result.Failure("Not found")` when not found
- Don't use nullable in Result unless data can legitimately be null

### 5. Authorization Flow
**Risk:** Where does authorization live?

**Solution:**
- Resource ownership checks → Service (returns Result with unauthorized error)
- Authentication checks → Controller attributes (still use [Authorize])
- `GetUserId()` → Controller, pass to service

---

## 11. Estimated Effort

| Phase | Files Changed | Lines Modified | Estimated Time |
|-------|---------------|----------------|----------------|
| Phase 1: Auth | 3 | ~150 | 4-6 hours |
| Phase 2: ShareLinks | 4 | ~200 | 6-8 hours |
| Phase 3: Albums | 4 | ~180 | 5-7 hours |
| Phase 4: Tags | 4 | ~120 | 3-4 hours |
| Phase 5: Photos | 4 | ~250 | 8-10 hours |
| Testing | All | ~400 | 8-12 hours |
| **Total** | **19** | **~1300** | **34-47 hours** |

**Timeline (Sprint-based):**
- Sprint 1 (Week 1): Phase 1 + Phase 2
- Sprint 2 (Week 2): Phase 3 + Phase 4
- Sprint 3 (Week 3): Phase 5 + Testing

---

## 12. Rollout Strategy

### Versioning Plan

- **v1.2.0**: Auth refactored
- **v1.2.1**: ShareLinks refactored
- **v1.2.2**: Albums refactored
- **v1.2.3**: Tags refactored
- **v1.2.4**: Photos refactored (complete)

### Backward Compatibility

**API Contracts:** No breaking changes. Same request/response shapes.

**Internal:** Tuples and Results can coexist during transition.

### Rollback Plan

Each phase is independently reversible:
- Git revert per phase
- Database migrations not affected
- Client changes not required

---

## 13. Success Criteria

### Code Quality
- [ ] No business logic in controllers
- [ ] All services return Result types
- [ ] No exceptions thrown for validation
- [ ] Consistent error response format
- [ ] All tests passing

### Observability
- [ ] Structured logging for all failures
- [ ] Error messages are user-friendly
- [ ] Audit trail for auth operations

### Developer Experience
- [ ] Clear pattern for new features
- [ ] Extension methods reduce boilerplate
- [ ] Documentation updated

---

## 14. Open Questions

1. **Error Localization:** Should we support multiple languages for error messages?
   - Recommendation: Defer to v1.3

2. **Error Codes:** Should we include error codes in API responses?
   - Recommendation: Add to ErrorResponse but optional for now

3. **Validation Framework:** Should we use FluentValidation with Results?
   - Recommendation: Keep manual validation for simplicity

4. **Logging Level:** Should all Result failures be logged as warnings?
   - Recommendation: Log based on severity (security warnings vs validation info)

5. **Test Coverage:** What % coverage target for refactored code?
   - Recommendation: 80% for services, 70% overall

---

## 15. References

- [CSharpFunctionalExtensions GitHub](https://github.com/vkhorikov/CSharpFunctionalExtensions)
- [Result Pattern Explanation](https://enterprisecraftsmanship.com/posts/functional-csharp-success-result/)
- [Clean Architecture Result Pattern](https://blog.ploeh.dk/2022/04/19/the-result-pattern-is-not-a-domain-model-pattern/)

---

## Appendix: Code Examples

### Complete Service Example

```csharp
// IAlbumService.cs
public interface IAlbumService
{
    Task<Result<AlbumResponse>> CreateAlbumAsync(string userId, CreateAlbumRequest request, CancellationToken cancellationToken);
    Task<Result<AlbumDetailsResponse>> GetAlbumAsync(string userId, Guid albumId, CancellationToken cancellationToken);
    Task<Result> UpdateAlbumAsync(string userId, Guid albumId, UpdateAlbumRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAlbumAsync(string userId, Guid albumId, CancellationToken cancellationToken);
    Task<Result> AddPhotoAsync(string userId, Guid albumId, Guid photoId, CancellationToken cancellationToken);
}

// AlbumService.cs
public class AlbumService : IAlbumService
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IPhotoRepository _photoRepository;

    public async Task<Result<AlbumResponse>> CreateAlbumAsync(string userId, CreateAlbumRequest request, CancellationToken cancellationToken)
    {
        var album = new Album
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _albumRepository.AddAsync(album, cancellationToken);

        return Result.Success(new AlbumResponse
        {
            Id = album.Id,
            Name = album.Name,
            Description = album.Description,
            CreatedAt = album.CreatedAt,
            UpdatedAt = album.UpdatedAt,
            PhotoCount = 0
        });
    }

    public async Task<Result<AlbumDetailsResponse>> GetAlbumAsync(string userId, Guid albumId, CancellationToken cancellationToken)
    {
        var album = await _albumRepository.GetByIdAsync(albumId, cancellationToken);

        if (album == null)
            return Result.Failure<AlbumDetailsResponse>(Errors.Albums.NotFound);

        if (album.UserId != userId)
            return Result.Failure<AlbumDetailsResponse>(Errors.Albums.UnauthorizedAccess);

        return Result.Success(new AlbumDetailsResponse
        {
            Id = album.Id,
            Name = album.Name,
            Description = album.Description,
            Photos = album.AlbumPhotos.Select(ap => MapToPhotoResponse(ap.Photo)).ToList()
        });
    }
}

// AlbumsController.cs
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AlbumsController : ApiControllerBase
{
    private readonly IAlbumService _albumService;

    [HttpPost]
    public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumRequest request, CancellationToken cancellationToken)
        => await _albumService.CreateAlbumAsync(GetUserId(), request, cancellationToken)
            .ToActionResult();

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAlbum(Guid id, CancellationToken cancellationToken)
        => await _albumService.GetAlbumAsync(GetUserId(), id, cancellationToken)
            .ToNotFoundResult(Errors.Albums.NotFound);
}
```

### Authorization Pattern

```csharp
// Service handles authorization via Result
public async Task<Result<Photo>> GetPhotoAsync(string userId, Guid photoId, CancellationToken cancellationToken)
{
    var photo = await _photoRepository.GetByIdAsync(photoId, cancellationToken);

    if (photo == null)
        return Result.Failure<Photo>(Errors.Photos.NotFound);

    if (photo.UserId != userId)
        return Result.Failure<Photo>(Errors.Photos.UnauthorizedAccess);

    return Result.Success(photo);
}

// Controller maps to appropriate status
[HttpGet("{id}")]
public async Task<IActionResult> GetPhoto(Guid id, CancellationToken cancellationToken)
{
    var result = await _photoService.GetPhotoAsync(GetUserId(), id, cancellationToken);

    if (result.IsFailure && result.Error == Errors.Photos.UnauthorizedAccess)
        return Forbid();

    return result.Match(
        onSuccess: photo => Ok(MapToResponse(photo)),
        onFailure: error => NotFound(new { error })
    );
}
```

### Chaining Results

```csharp
public async Task<Result> AddPhotoToAlbumAsync(string userId, Guid albumId, Guid photoId, CancellationToken cancellationToken)
{
    // Validate album exists and user owns it
    var albumResult = await ValidateAlbumOwnershipAsync(userId, albumId, cancellationToken);
    if (albumResult.IsFailure)
        return Result.Failure(albumResult.Error);

    // Validate photo exists and user owns it
    var photoResult = await ValidatePhotoOwnershipAsync(userId, photoId, cancellationToken);
    if (photoResult.IsFailure)
        return Result.Failure(photoResult.Error);

    // Check if photo already in album
    if (await _albumRepository.IsPhotoInAlbumAsync(albumId, photoId, cancellationToken))
        return Result.Failure("Photo is already in this album");

    // Add to album
    var sortOrder = await _albumRepository.GetPhotoCountAsync(albumId, cancellationToken);
    await _albumRepository.AddPhotoToAlbumAsync(albumId, photoId, sortOrder, cancellationToken);

    return Result.Success();
}

private async Task<Result<Album>> ValidateAlbumOwnershipAsync(string userId, Guid albumId, CancellationToken cancellationToken)
{
    var album = await _albumRepository.GetByIdAsync(albumId, cancellationToken);
    if (album == null)
        return Result.Failure<Album>(Errors.Albums.NotFound);
    if (album.UserId != userId)
        return Result.Failure<Album>(Errors.Albums.UnauthorizedAccess);
    return Result.Success(album);
}
```
