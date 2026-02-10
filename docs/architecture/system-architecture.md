# System Architecture - MyPhotoBooth v1.3.0

## Table of Contents

1. [Overview](#overview)
2. [Clean Architecture with CQRS](#clean-architecture-with-cqrs)
3. [Technology Stack](#technology-stack)
4. [Project Structure](#project-structure)
5. [CQRS Pattern Implementation](#cqrs-pattern-implementation)
6. [Pipeline Behaviors](#pipeline-behaviors)
7. [Validation Strategy](#validation-strategy)
8. [Error Handling](#error-handling)
9. [Testing Architecture](#testing-architecture)
10. [Data Flow](#data-flow)
11. [Security](#security)
12. [Performance Considerations](#performance-considerations)

## Overview

MyPhotoBooth is a full-stack photo memories application built with Clean Architecture principles, CQRS pattern, and comprehensive testing. The application separates concerns into distinct layers while maintaining clear boundaries and dependencies.

### Key Architectural Principles

1. **Separation of Concerns**: Each layer has specific responsibilities
2. **Dependency Inversion**: Dependencies point inward toward the domain
3. **CQRS**: Separation of read and write operations
4. **Testability**: Comprehensive unit and integration tests
5. **SOLID Principles**: Single responsibility, open/closed, Liskov substitution, interface segregation, dependency inversion

## Clean Architecture with CQRS

### Layer Structure

```
┌─────────────────────────────────────────────────────────────┐
│                      Presentation Layer                      │
│                    (MyPhotoBooth.API)                        │
│  Controllers • Endpoints • Middleware • Filters              │
└────────────────────────┬────────────────────────────────────┘
                         │ ISender.Send()
┌────────────────────────▼────────────────────────────────────┐
│                     Application Layer                        │
│                 (MyPhotoBooth.Application)                   │
│  Commands • Queries • Handlers • Validators • DTOs          │
│  Pipeline Behaviors • Interfaces                             │
└────────────────────────┬────────────────────────────────────┘
                         │ Repository Interfaces
┌────────────────────────▼────────────────────────────────────┐
│                  Infrastructure Layer                        │
│              (MyPhotoBooth.Infrastructure)                   │
│  EF Core • Repositories • Services • External Integrations  │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                      Domain Layer                            │
│                 (MyPhotoBooth.Domain)                        │
│  Entities • Value Objects • Domain Logic                     │
└─────────────────────────────────────────────────────────────┘
```

### CQRS Organization

The Application layer uses a feature-based structure:

```
Application/
├── Features/
│   ├── Auth/
│   │   ├── Commands/
│   │   │   ├── RegisterCommand.cs
│   │   │   ├── LoginCommand.cs
│   │   │   ├── LogoutCommand.cs
│   │   │   ├── RefreshTokenCommand.cs
│   │   │   ├── ForgotPasswordCommand.cs
│   │   │   └── ResetPasswordCommand.cs
│   │   ├── Queries/
│   │   │   └── GetCurrentUserQuery.cs
│   │   ├── Handlers/
│   │   │   ├── RegisterCommandHandler.cs
│   │   │   ├── LoginCommandHandler.cs
│   │   │   └── ...
│   │   └── Validators/
│   │       ├── RegisterCommandValidator.cs
│   │       ├── LoginCommandValidator.cs
│   │       └── ...
│   ├── Photos/
│   ├── Albums/
│   ├── Tags/
│   └── ShareLinks/
└── Common/
    ├── Behaviors/
    ├── DTOs/
    ├── Requests/
    ├── Validators/
    └── Pagination/
```

## Technology Stack

### Backend

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core | 10.0 |
| Database | PostgreSQL | 16 |
| Authentication | ASP.NET Identity | 10.0 |
| CQRS | MediatR | 14.0 |
| Validation | FluentValidation | 12.1 |
| Result Pattern | CSharpFunctionalExtensions | Latest |
| Image Processing | SixLabors.ImageSharp | Latest |
| API Documentation | Scalar | Latest |

### Testing

| Component | Technology | Version |
|-----------|-----------|---------|
| Test Framework | xUnit | 2.9 |
| Mocking | Moq | 4.20 |
| Assertions | FluentAssertions | 8.8 |
| Integration Tests | Testcontainers | 4.10 |
| Code Coverage | Coverlet | 6.0 |

### Frontend

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | React | 18 |
| Language | TypeScript | 5 |
| Build Tool | Vite | 5 |
| State Management | Zustand | 4 |
| Server State | TanStack Query | 5 |
| HTTP Client | Axios | 1 |
| Routing | React Router | 6 |

## Project Structure

### Solution Layout

```
MyPhotoBooth/
├── src/
│   ├── MyPhotoBooth.API/           # Presentation Layer
│   │   ├── Controllers/            # API Controllers (thin wrappers)
│   │   ├── Middleware/             # Custom middleware
│   │   ├── Filters/                # Exception filters
│   │   └── Program.cs              # Application entry point
│   ├── MyPhotoBooth.Application/   # Application Layer
│   │   ├── Features/               # Feature-based organization
│   │   └── Common/                 # Shared components
│   ├── MyPhotoBooth.Infrastructure/# Infrastructure Layer
│   │   ├── Persistence/            # EF Core, DbContext
│   │   ├── Repositories/           # Repository implementations
│   │   ├── Services/               # External services
│   │   └── Identity/               # Identity configuration
│   ├── MyPhotoBooth.Domain/        # Domain Layer
│   │   ├── Entities/               # Domain entities
│   │   └── Interfaces/             # Repository interfaces
│   └── client/                     # Frontend
├── tests/
│   ├── MyPhotoBooth.UnitTests/     # Unit tests
│   └── MyPhotoBooth.IntegrationTests/ # Integration tests
```

## CQRS Pattern Implementation

### Command Pattern (Write Operations)

Commands represent intentions to change system state:

```csharp
// Define command
public record CreatePhotoCommand(
    IFormFile File,
    string? Title,
    string? Description,
    string? Caption,
    DateTime? TakenAt,
    double? Latitude,
    double? Longitude
) : ICommand<PhotoDto>;

// Define validator
public class CreatePhotoCommandValidator : AbstractValidator<CreatePhotoCommand>
{
    public CreatePhotoCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage(Errors.Photos.NoFileUploaded)
            .Must(BeValidImageFile).WithMessage(Errors.Photos.InvalidFile)
            .Must(BeWithinSizeLimit).WithMessage(Errors.Photos.FileTooLarge);
    }
}

// Define handler
public class CreatePhotoCommandHandler : ICommandHandler<CreatePhotoCommand, PhotoDto>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUserContext _userContext;

    public async Task<Result<PhotoDto>> Handle(
        CreatePhotoCommand request,
        CancellationToken cancellationToken)
    {
        // Business logic here
        // Return Result.Success(photoDto) or Result.Failure("error")
    }
}
```

### Query Pattern (Read Operations)

Queries represent requests for data:

```csharp
// Define query
public record GetPhotosQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? AlbumId = null,
    Guid? TagId = null
) : IQuery<PaginatedResult<PhotoDto>>;

// Define validator
public class GetPhotosQueryValidator : AbstractValidator<GetPhotosQuery>
{
    public GetPhotosQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

// Define handler
public class GetPhotosQueryHandler : IQueryHandler<GetPhotosQuery, PaginatedResult<PhotoDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IUserContext _userContext;

    public async Task<Result<PaginatedResult<PhotoDto>>> Handle(
        GetPhotosQuery request,
        CancellationToken cancellationToken)
    {
        // Query logic here
        // Return paginated results
    }
}
```

### Controller Pattern

Controllers are thin wrappers that dispatch to MediatR:

```csharp
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly ISender _sender;

    public PhotosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<ActionResult<PhotoDto>> CreatePhoto(
        [FromForm] CreatePhotoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetPhoto),
            new { id = result.Value.Id },
            result.Value);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<PhotoDto>>> GetPhotos(
        [FromQuery] GetPhotosQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
```

## Pipeline Behaviors

Pipeline behaviors provide cross-cutting concerns for all requests/commands.

### Behavior Chain

```
Request → Validation → Logging → Transaction → Handler → Response
           Behavior     Behavior    Behavior
```

### 1. ValidationBehavior

**Purpose**: Validates all requests using FluentValidation before handlers execute

**Execution**: First in chain

```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Get all validators for the request type
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        // Collect errors
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(); // Continue to next behavior
        }

        // Return Result.Failure with validation errors
        return Result.Failure(errorMessage);
    }
}
```

**Benefits**:
- Automatic validation for all commands/queries
- Consistent error responses
- No need for manual validation in handlers
- Single responsibility for validation logic

### 2. LoggingBehavior

**Purpose**: Logs all requests and responses with timing information

**Execution**: Second in chain

```csharp
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {RequestName}: {@Request}",
            typeof(TRequest).Name,
            request);

        try
        {
            var response = await next();

            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error handling {RequestName} after {ElapsedMs}ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
```

**Benefits**:
- Automatic request/response logging
- Performance monitoring
- Debugging support
- Audit trail

### 3. TransactionBehavior

**Purpose**: Wraps handlers in database transactions for data consistency

**Execution**: Third in chain (last before handler)

```csharp
public class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only use transactions for commands (write operations)
        if (typeof(TRequest).Name.EndsWith("Query"))
        {
            return await next();
        }

        using var transaction = await _context.Database.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var response = await next();

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return response;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
```

**Benefits**:
- Automatic transaction management
- Data consistency across multiple operations
- Automatic rollback on failure
- No manual transaction code in handlers

## Validation Strategy

### FluentValidation Integration

Validation is declarative and separate from business logic:

1. **Validators are auto-discovered** by FluentValidation DI extensions
2. **ValidationBehavior** intercepts all requests
3. **Validation errors** return consistent error responses
4. **Custom validators** can be reused across commands/queries

### Custom Validators

Reusable validators for common validation scenarios:

```csharp
public class EmailValidator<T> : PropertyValidator<T, string>
{
    public override string Name => "EmailValidator";

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "'{PropertyName}' must be a valid email address.";

    public override bool IsValid(ValidationContext<T> context,
        string value)
    {
        // Email validation logic
    }
}

public class StrongPasswordValidator<T> : PropertyValidator<T, string>
{
    public override string Name => "StrongPasswordValidator";

    public override bool IsValid(ValidationContext<T> context,
        string value)
    {
        // Strong password validation:
        // - At least 8 characters
        // - At least one uppercase letter
        // - At least one lowercase letter
        // - At least one digit
    }
}
```

### Validation Response Format

Validation errors return structured responses:

```json
{
  "error": "Email: Invalid email format; Password: Password must be at least 8 characters"
}
```

## Error Handling

### Result<T> Pattern

Using CSharpFunctionalExtensions for consistent error handling:

```csharp
// Success case
var photo = await _photoRepository.GetByIdAsync(id);
return Result.Success(photo.ToDto());

// Failure case
if (photo == null)
{
    return Result.Failure(Errors.Photos.NotFound);
}

// Usage in controller
var result = await _sender.Send(command);
if (result.IsFailure)
{
    return BadRequest(result.Error);
}
return Ok(result.Value);
```

### Centralized Error Constants

All error messages defined in one location:

```csharp
public static class Errors
{
    public static class Auth
    {
        public const string EmailAlreadyExists = "Email already registered";
        public const string InvalidCredentials = "Invalid email or password";
        public const string UserNotFound = "User not found";
        public const string InvalidToken = "Invalid or expired token";
    }

    public static class Photos
    {
        public const string NotFound = "Photo not found";
        public const string UnauthorizedAccess = "You do not have access to this photo";
        public const string InvalidFile = "Invalid image file";
    }

    // ... more error categories
}
```

## Testing Architecture

### Test Structure

```
tests/
├── MyPhotoBooth.UnitTests/
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── Validators/
│   │   │   │   ├── LoginCommandValidatorTests.cs
│   │   │   │   ├── RegisterCommandValidatorTests.cs
│   │   │   │   └── ForgotPasswordCommandValidatorTests.cs
│   │   │   └── Handlers/
│   │   │       └── LoginCommandHandlerTests.cs
│   │   ├── Photos/
│   │   │   ├── Validators/
│   │   │   │   ├── UploadPhotoCommandValidatorTests.cs
│   │   │   │   ├── UpdatePhotoCommandValidatorTests.cs
│   │   │   │   └── DeletePhotoCommandValidatorTests.cs
│   │   │   └── Handlers/
│   │   │       └── GetPhotosQueryHandlerTests.cs
│   │   ├── Albums/
│   │   │   └── Validators/
│   │   ├── Tags/
│   │   │   └── Validators/
│   │   └── ShareLinks/
│   │       └── Validators/
│   ├── Common/
│   │   └── Behaviors/
│   │       └── ValidationBehaviorTests.cs
│   └── Helpers/
│       └── TestHelpers.cs
└── MyPhotoBooth.IntegrationTests/
    ├── Features/
    │   ├── Auth/
    │   │   ├── AuthEndpointTests.cs
    │   │   └── LoginEndpointTests.cs
    │   ├── Albums/
    │   │   └── AlbumsEndpointTests.cs
    │   ├── ShareLinks/
    │   │   └── ShareLinksEndpointTests.cs
    │   └── Tags/
    │       └── TagsEndpointTests.cs
    ├── Fixtures/
    │   ├── PostgreSqlFixture.cs
    │   └── TestWebApplicationFactory.cs
    └── Helpers/
        └── TestAuthHelper.cs
```

### Unit Tests (86 tests)

**Focus**: Individual component testing

**What's tested**:
- Validators: All validation rules and edge cases
- Behaviors: Pipeline behavior execution
- Handlers: Business logic with mocked dependencies

**Example**:

```csharp
public class LoginCommandValidatorTests
{
    [Fact]
    public void Should_Have_Error_When_Email_IsInvalid()
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("invalid-email", "Password123!");

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Should_Return_Success_When_Credentials_AreValid()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var handler = new LoginCommandHandler(mockRepo.Object, ...);
        var command = new LoginCommand("test@example.com", "Password123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

### Integration Tests (31 tests)

**Focus**: End-to-end API testing

**What's tested**:
- Full request/response cycle
- Database operations with Testcontainers
- Authentication and authorization
- Error responses

**Example**:

```csharp
public class AuthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClient();
        var loginRequest = new LoginRequest("test@example.com", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeEmpty();
    }
}
```

### Test Coverage

- **Validators**: ~70% coverage
- **Handlers**: ~10% coverage
- **Behaviors**: 100% coverage
- **API Endpoints**: 100% coverage

### Running Tests

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/MyPhotoBooth.UnitTests

# Run integration tests only
dotnet test tests/MyPhotoBooth.IntegrationTests

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "FullyQualifiedName~LoginCommandValidatorTests"
```

## Data Flow

### Request Flow

```
1. Client sends HTTP request
   ↓
2. API Controller receives request
   ↓
3. Controller creates Command/Query
   ↓
4. Controller calls _sender.Send(command)
   ↓
5. ValidationBehavior validates request
   ↓ (if valid)
6. LoggingBehavior logs request
   ↓
7. TransactionBehavior begins transaction (for commands)
   ↓
8. Handler executes business logic
   ↓
9. Handler returns Result<T>
   ↓
10. TransactionBehavior commits transaction
   ↓
11. LoggingBehavior logs response
   ↓
12. Controller returns HTTP response
   ↓
13. Client receives response
```

### Example: Upload Photo

```
POST /api/photos
Content-Type: multipart/form-data

1. PhotosController.CreatePhoto() receives request
2. Creates CreatePhotoCommand
3. Calls _sender.Send(command)
4. ValidationBehavior validates file format and size
5. LoggingBehavior logs "Handling CreatePhotoCommand"
6. TransactionBehavior begins transaction
7. CreatePhotoCommandHandler:
   a. Reads file stream
   b. Processes image (auto-rotate, extract EXIF)
   c. Generates thumbnail
   d. Stores files
   e. Creates Photo entity
   f. Saves to database
8. TransactionBehavior commits transaction
9. LoggingBehavior logs "Handled CreatePhotoCommand in 245ms"
10. Controller returns 201 Created with PhotoDto
11. Client receives photo details with URLs
```

## Security

### Authentication Flow

1. **Registration**:
   - User submits email, password, display name
   - Password hashed using ASP.NET Identity
   - User created in database
   - JWT tokens generated

2. **Login**:
   - User submits credentials
   - Password verified against hash
   - Access token (15 min) + Refresh token (7 days) generated
   - Tokens returned to client

3. **Token Refresh**:
   - Client sends refresh token
   - Token validated against database
   - New access token generated
   - Old refresh token revoked (rotation)

4. **Authorization**:
   - Protected endpoints require [Authorize]
   - JWT token validated on each request
   - User context extracted from token claims

### Security Best Practices

1. **Password Requirements**:
   - Minimum 8 characters
   - At least one uppercase letter
   - At least one lowercase letter
   - At least one digit

2. **Token Security**:
   - Access tokens expire quickly (15 minutes)
   - Refresh tokens stored in database
   - Refresh token rotation prevents reuse
   - Tokens signed with secret key

3. **User Enumeration Prevention**:
   - Forgot password returns success for non-existent emails
   - Login returns generic error for invalid credentials

4. **File Upload Security**:
   - File type validation (whitelist)
   - File size limits
   - EXIF data sanitization
   - Storage outside web root

5. **CORS Configuration**:
   - Specific origins allowed
   - Credentials allowed
   - Specific headers allowed

## Performance Considerations

### Database Optimization

1. **Indexes**:
   - Foreign keys indexed
   - frequently queried columns indexed
   - Composite indexes for common queries

2. **Query Optimization**:
   - AsNoTracking() for read-only queries
   - Include() for eager loading
   - Pagination to limit result sets

3. **Connection Pooling**:
   - EF Core manages connection pool
   - Configured pool size in connection string

### Caching Strategy

1. **Response Caching**:
   - API responses can be cached
   - Cache-Control headers set appropriately

2. **Thumbnail Caching**:
   - Thumbnails generated once and stored
   - Served directly from storage

### File Storage

1. **Local File System**:
   - Organized structure: `{userId}/{year}/{month}/`
   - Thumbnails in separate location
   - Efficient file serving

2. **Future: Cloud Storage**:
   - Can migrate to S3, Azure Blob, etc.
   - Interface-based design allows easy swap

### Frontend Optimization

1. **Image Loading**:
   - Lazy loading for gallery
   - Thumbnail preview, full image on demand
   - Blob URLs for authenticated images

2. **State Management**:
   - Server state: TanStack Query with caching
   - Client state: Zustand for UI state

3. **Code Splitting**:
   - Route-based code splitting with React Router
   - Lazy loading components

## Monitoring and Observability

### Logging

- Structured logging with Microsoft.Extensions.Logging
- Log levels: Trace, Debug, Information, Warning, Error, Critical
- Logs include: Request type, user ID, timing, errors

### Performance Metrics

- Request/response timing in LoggingBehavior
- Database query timing
- Slow query detection

### Health Checks

- `/health` endpoint for container orchestration
- Database connectivity check
- Optional: Disk space check for storage

## Future Enhancements

1. **Caching Layer**: Redis for distributed caching
2. **Message Queue**: Background processing for image processing
3. **Read Models**: CQRS with separate read models for queries
4. **Event Sourcing**: Audit trail with event sourcing
5. **API Versioning**: Versioning strategy for breaking changes
6. **Rate Limiting**: API rate limiting per user
7. **GraphQL**: Alternative to REST for flexible queries
8. **Real-time**: SignalR for real-time updates

---

**Document Version**: 1.0
**Last Updated**: 2025-02-10
**MyPhotoBooth Version**: 1.3.0
