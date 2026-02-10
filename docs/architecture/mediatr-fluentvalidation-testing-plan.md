# MediatR, FluentValidation, and Comprehensive Testing Integration Plan

## Executive Summary

Plan for integrating **MediatR** (CQRS + Mediator pattern), **FluentValidation** (declarative validation), and **comprehensive testing** into MyPhotoBooth while maintaining existing functionality and following Clean Architecture principles.

**Status**: Ready for implementation
**Version**: 1.0
**Last Updated**: 2026-02-10

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Package Installation Strategy](#package-installation-strategy)
3. [Folder Structure Reorganization](#folder-structure-reorganization)
4. [MediatR Integration](#mediatr-integration)
5. [FluentValidation Integration](#fluentvalidation-integration)
6. [Testing Strategy](#testing-strategy)
7. [Migration Plan](#migration-plan)
8. [Code Examples](#code-examples)
9. [Effort Estimation](#effort-estimation)
10. [Potential Pitfalls](#potential-pitfalls)

---

## Architecture Overview

### Current State

```
┌─────────────┐
│  Controller │ ───> Service Interface ───> Service Implementation ───> Repository
└─────────────┘
```

### Target State

```
┌─────────────┐
│  Controller │ ───> Mediator.Send() ───> Handler ───> Repository
└─────────────┘                          │
                                         └──> Pipeline Behaviors
                                              ├─> Validation
                                              ├─> Logging
                                              └─> Transactions
```

### Key Benefits

1. **Decoupling**: Controllers don't depend on service implementations
2. **CQRS**: Separate command (write) and query (read) models
3. **Cross-cutting concerns**: Centralized via pipeline behaviors
4. **Testability**: Handlers are easily testable in isolation
5. **Open/Closed**: New features = new handlers, not changes to existing code

---

## Package Installation Strategy

### Required NuGet Packages

| Project | Package | Purpose |
|---------|---------|---------|
| `MyPhotoBooth.Application` | `MediatR` | Core mediator pattern |
| `MyPhotoBooth.Application` | `FluentValidation` | Validation framework |
| `MyPhotoBooth.Application` | `FluentValidation.DependencyInjection` | DI integration |
| `MyPhotoBooth.Application` | `Microsoft.Extensions.Logging.Abstractions` | Already present |
| `MyPhotoBooth.API` | `MediatR` | Controllers send requests |
| `MyPhotoBooth.UnitTests` | `xunit` | Test framework |
| `MyPhotoBooth.UnitTests` | `xunit.runner.visualstudio` | Test runner |
| `MyPhotoBooth.UnitTests` | `FluentAssertions` | Fluent assertions |
| `MyPhotoBooth.UnitTests` | `Moq` | Mocking framework |
| `MyPhotoBooth.UnitTests` | `MediatR` | Test handlers |
| `MyPhotoBooth.UnitTests` | `FluentValidation` | Test validators |
| `MyPhotoBooth.IntegrationTests` | `Microsoft.AspNetCore.Mvc.Testing` | WebApplicationFactory |
| `MyPhotoBooth.IntegrationTests` | `Testcontainers.PostgreSql` | PostgreSQL container |

### Installation Commands

```bash
# Core packages
dotnet add src/MyPhotoBooth.Application package MediatR
dotnet add src/MyPhotoBooth.Application package FluentValidation
dotnet add src/MyPhotoBooth.Application package FluentValidation.DependencyInjection

# API project
dotnet add src/MyPhotoBooth.API package MediatR

# Unit tests (new project)
dotnet new xunit -n MyPhotoBooth.UnitTests -o tests/MyPhotoBooth.UnitTests
dotnet add tests/MyPhotoBooth.UnitTests reference src/MyPhotoBooth.Application
dotnet add tests/MyPhotoBooth.UnitTests package FluentAssertions
dotnet add tests/MyPhotoBooth.UnitTests package Moq
dotnet add tests/MyPhotoBooth.UnitTests package MediatR
dotnet add tests/MyPhotoBooth.UnitTests package FluentValidation

# Integration tests (new project)
dotnet new xunit -n MyPhotoBooth.IntegrationTests -o tests/MyPhotoBooth.IntegrationTests
dotnet add tests/MyPhotoBooth.IntegrationTests reference src/MyPhotoBooth.API
dotnet add tests/MyPhotoBooth.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/MyPhotoBooth.IntegrationTests package Testcontainers.PostgreSql
dotnet add tests/MyPhotoBooth.IntegrationTests package FluentAssertions

# Add to solution
dotnet sln add tests/MyPhotoBooth.UnitTests/MyPhotoBooth.UnitTests.csproj
dotnet sln add tests/MyPhotoBooth.IntegrationTests/MyPhotoBooth.IntegrationTests.csproj
```

---

## Folder Structure Reorganization

### Current Structure

```
src/
├── MyPhotoBooth.API/
├── MyPhotoBooth.Application/
│   ├── Common/
│   │   ├── DTOs/
│   │   └── Errors.cs
│   └── Interfaces/
├── MyPhotoBooth.Infrastructure/
└── MyPhotoBooth.Domain/
```

### Proposed Structure

```
src/
├── MyPhotoBooth.API/
│   └── Controllers/          # Thin HTTP layer
├── MyPhotoBooth.Application/
│   ├── Common/               # Shared utilities
│   │   ├── Behaviors/        # Pipeline behaviors
│   │   ├── DTOs/            # Response DTOs
│   │   ├── Errors.cs        # Error constants
│   │   └── Results/         # Result types/extensions
│   ├── Features/            # Feature-based organization
│   │   ├── Photos/
│   │   │   ├── Commands/    # Write operations
│   │   │   │   ├── UploadPhotoCommand.cs
│   │   │   │   ├── UpdatePhotoCommand.cs
│   │   │   │   ├── DeletePhotoCommand.cs
│   │   │   │   └── AddPhotoToAlbumCommand.cs
│   │   │   ├── Queries/     # Read operations
│   │   │   │   ├── GetPhotoQuery.cs
│   │   │   │   ├── ListPhotosQuery.cs
│   │   │   │   ├── GetPhotoTimelineQuery.cs
│   │   │   │   └── GetPhotoFileQuery.cs
│   │   │   ├── Validators/  # FluentValidation validators
│   │   │   │   ├── UploadPhotoCommandValidator.cs
│   │   │   │   ├── UpdatePhotoCommandValidator.cs
│   │   │   │   └── ListPhotosQueryValidator.cs
│   │   │   └── Handlers/    # Request handlers
│   │   │       ├── UploadPhotoCommandHandler.cs
│   │   │       ├── GetPhotoQueryHandler.cs
│   │   │       └── ...
│   │   ├── Albums/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   ├── Validators/
│   │   │   └── Handlers/
│   │   ├── Tags/
│   │   ├── Auth/
│   │   └── ShareLinks/
│   └── Interfaces/          # External service contracts
├── MyPhotoBooth.Infrastructure/
│   └── ... (unchanged)
└── MyPhotoBooth.Domain/
    └── Entities/            # Domain entities (unchanged)

tests/
├── MyPhotoBooth.UnitTests/
│   ├── Features/
│   │   ├── Photos/
│   │   │   ├── Commands/
│   │   │   │   ├── UploadPhotoCommandHandlerTests.cs
│   │   │   │   └── UpdatePhotoCommandHandlerTests.cs
│   │   │   ├── Queries/
│   │   │   │   ├── GetPhotoQueryHandlerTests.cs
│   │   │   │   └── ListPhotosQueryHandlerTests.cs
│   │   │   └── Validators/
│   │   │       └── UploadPhotoCommandValidatorTests.cs
│   │   ├── Albums/
│   │   ├── Tags/
│   │   ├── Auth/
│   │   └── ShareLinks/
│   └── Common/
│       └── TestHelpers.cs
└── MyPhotoBooth.IntegrationTests/
    ├── Features/
    │   ├── Photos/
    │   │   └── PhotosEndpointTests.cs
    │   ├── Albums/
    │   └── Auth/
    ├── Fixtures/
    │   ├── PostgreSqlFixture.cs
    │   └── ApiWebApplicationFactory.cs
    └── TestData/
        └── SeedData.cs
```

---

## MediatR Integration

### Naming Conventions

| Type | Naming Pattern | Example |
|------|----------------|---------|
| Command | `{Verb}{Noun}Command` | `UploadPhotoCommand` |
| Query | `{Verb}{Noun}Query` | `GetPhotoQuery` |
| Handler | `{Request}Handler` | `UploadPhotoCommandHandler` |
| Validator | `{Request}Validator` | `UploadPhotoCommandValidator` |

### Request Base Types

```csharp
// Application/Common/Requests/IRequest.cs
namespace MyPhotoBooth.Application.Common.Requests;

public interface IRequest<TResult> : MediatR.IRequest<Result<TResult>>
{
}

public interface ICommand<TResult> : IRequest<TResult>
{
}

public interface IQuery<TResult> : IRequest<TResult>
{
}

// For commands that return no data
public interface ICommand : MediatR.IRequest<Result>
{
}
```

### Response Type Standardization

```csharp
// Application/Common/Results/ResultExtensions.cs
using CSharpFunctionalExtensions;

namespace MyPhotoBooth.Application.Common.Results;

public static class ResultExtensions
{
    public static IActionResult ToHttpResponse<TResult>(this Result<TResult> result)
    {
        if (result.IsSuccess) return new OkObjectResult(result.Value);

        return result.Error switch
        {
            var e when e.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => new NotFoundObjectResult(new { message = e }),
            var e when e.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
                => new UnauthorizedObjectResult(new { message = e }),
            _ => new BadRequestObjectResult(new { message = result.Error })
        };
    }

    public static IActionResult ToHttpResponse(this Result result)
    {
        if (result.IsSuccess) return new NoContentResult();

        return result.Error switch
        {
            var e when e.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => new NotFoundObjectResult(new { message = e }),
            var e when e.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)
                => new UnauthorizedObjectResult(new { message = e }),
            _ => new BadRequestObjectResult(new { message = result.Error })
        };
    }
}
```

### Pipeline Behaviors

#### 1. Validation Behavior

```csharp
// Application/Common/Behaviors/ValidationBehavior.cs
using FluentValidation;
using MediatR;
using CSharpFunctionalExtensions;

namespace MyPhotoBooth.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var validationResults = await Task.WhenAll(
            _validators.Select(v =>
                v.ValidateAsync(request, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            _logger.LogWarning(
                "Validation failed for {RequestType}: {Errors}",
                typeof(TRequest).Name,
                errors);

            // Create validation failure result
            var errorMessage = string.Join("; ",
                failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));

            // This requires TResponse to be Result or Result<T>
            var resultType = typeof(TResponse);
            var failureMethod = resultType.GetMethod(
                "Failure",
                new[] { typeof(string) });

            if (failureMethod != null)
            {
                return (TResponse)failureMethod.Invoke(null, new object[] { errorMessage })!;
            }
        }

        return await next();
    }
}
```

#### 2. Logging Behavior

```csharp
// Application/Common/Behaviors/LoggingBehavior.cs
using MediatR;
using System.Diagnostics;

namespace MyPhotoBooth.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        _logger.LogInformation(
            "Handling {RequestName} (RequestId: {RequestId}): {@Request}",
            requestName, requestId, request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Handled {RequestName} (RequestId: {RequestId}) in {ElapsedMs}ms",
                requestName, requestId, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} (RequestId: {RequestId}) after {ElapsedMs}ms: {@Request}",
                requestName, requestId, stopwatch.ElapsedMilliseconds, request);

            throw;
        }
    }
}
```

#### 3. Transaction Behavior

```csharp
// Application/Common/Behaviors/TransactionBehavior.cs
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using MyPhotoBooth.Infrastructure.Persistence;

namespace MyPhotoBooth.Application.Common.Behaviors;

public class TransactionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        AppDbContext dbContext,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only use transactions for commands (writes)
        if (!typeof(TRequest).Name.EndsWith("Command"))
        {
            return await next();
        }

        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

            _logger.LogDebug(
                "Started transaction for {RequestType}",
                typeof(TRequest).Name);

            var response = await next();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogDebug(
                "Committed transaction for {RequestType}",
                typeof(TRequest).Name);

            return response;
        }
        catch (Exception)
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogWarning(
                    "Rolled back transaction for {RequestType}",
                    typeof(TRequest).Name);
            }
            throw;
        }
        finally
        {
            transaction?.Dispose();
        }
    }
}
```

### DI Registration

```csharp
// Infrastructure/DependencyInjection.cs (updated)
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... existing code ...

    // MediatR
    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
    });

    // FluentValidation
    services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

    // Pipeline behaviors (order matters!)
    services.AddTransient(
        typeof(IPipelineBehavior<,>),
        typeof(ValidationBehavior<,>));
    services.AddTransient(
        typeof(IPipelineBehavior<,>),
        typeof(LoggingBehavior<,>));
    services.AddTransient(
        typeof(IPipelineBehavior<,>),
        typeof(TransactionBehavior<,>));

    return services;
}
```

### Controller Pattern

```csharp
// API/Controllers/PhotosController.cs (updated)
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ApiControllerBase
{
    private readonly ISender _mediator;

    public PhotosController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(
        IFormFile file,
        [FromForm] string? description,
        CancellationToken cancellationToken)
    {
        var command = new UploadPhotoCommand(file, description);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhoto(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPhotoQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }
}
```

---

## FluentValidation Integration

### Validator Organization

Validators are co-located with their requests in the `Features/{Area}/Validators/` folder.

### Common Validators

```csharp
// Application/Common/Validators/SharedValidators.cs
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace MyPhotoBooth.Application.Common.Validators;

public static class SharedValidators
{
    public static IRuleBuilderOptions<T, string> Email<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email too long");
    }

    public static IRuleBuilderOptions<T, string> Password<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches("[0-9]").WithMessage("Password must contain digit");
    }

    public static IRuleBuilderOptions<T, IFormFile> ImageFile<T>(
        this IRuleBuilder<T, IFormFile> ruleBuilder,
        int maxSizeMB = 50)
    {
        return ruleBuilder
            .NotNull().WithMessage("File is required")
            .Must(f => f.Length > 0).WithMessage("File is empty")
            .Must(f => f.Length <= maxSizeMB * 1024 * 1024)
                .WithMessage($"File size exceeds {maxSizeMB}MB")
            .Must(f => f.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                .WithMessage("File must be an image");
    }
}
```

### Example Validators

```csharp
// Features/Photos/Validators/UploadPhotoCommandValidator.cs
using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class UploadPhotoCommandValidator : AbstractValidator<UploadPhotoCommand>
{
    public UploadPhotoCommandValidator()
    {
        RuleFor(x => x.File)
            .ImageFile(maxSizeMB: 50);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description too long")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

// Features/Photos/Validators/ListPhotosQueryValidator.cs
public class ListPhotosQueryValidator : AbstractValidator<ListPhotosQuery>
{
    public ListPhotosQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be positive");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");
    }
}
```

### Custom Validators for Complex Rules

```csharp
// Application/Common/Validators/CustomValidators.cs
using FluentValidation.Validators;

namespace MyPhotoBooth.Application.Common.Validators;

public class FutureDateValidator<T> : PropertyValidator<T, DateTime?>
{
    public override string Name => "FutureDateValidator";

    public override bool IsValid(ValidationContext<T> context, DateTime? value)
    {
        if (value is null) return true;
        return value <= DateTime.UtcNow;
    }

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "{PropertyName} cannot be in the future";
}

// Usage
public class CreateShareLinkCommandValidator : AbstractValidator<CreateShareLinkCommand>
{
    public CreateShareLinkCommandValidator()
    {
        RuleFor(x => x.ExpiresAt)
            .SetValidator(new FutureDateValidator<CreateShareLinkCommand>())
            .When(x => x.ExpiresAt.HasValue);
    }
}
```

---

## Testing Strategy

### Testing Pyramid

```
        /\
       /  \      E2E Tests (10%)
      /    \     - Critical user paths
     /------\
    /        \   Integration Tests (20%)
   /          \  - API endpoints
  /            \ - Database operations
 /--------------\
/                \ Unit Tests (70%)
                  - Handlers
                  - Validators
                  - Domain logic
```

### Unit Testing

#### Test Database Strategy: **In-Memory with Moq (Recommended)**

**Rationale:**
- **Speed**: Tests run 10-100x faster than with Docker containers
- **Simplicity**: No Docker dependencies, easier CI/CD
- **Adequate**: Repository pattern abstracts EF Core; we mock repos
- **Focus**: Tests business logic, not EF Core behavior

**Alternative (Docker Testcontainers):**
- Use if you need to test EF Core-specific queries
- Use if you have complex database-specific logic
- Slower but more production-like

#### Test Naming Convention

```
{UnitUnderTest}_{Scenario}_{ExpectedOutcome}

Examples:
- UploadPhotoCommandHandler_ValidFile_ReturnsSuccess
- UploadPhotoCommandHandler_FileTooLarge_ReturnsFailure
- GetPhotoQueryHandler_PhotoNotFound_ReturnsNotFound
- UploadPhotoCommandValidator_NoFile_ReturnsValidationError
```

#### Test Structure (AAA)

```csharp
// tests/MyPhotoBooth.UnitTests/Features/Photos/Commands/UploadPhotoCommandHandlerTests.cs
using FluentAssertions;
using Moq;
using Xunit;
using CSharpFunctionalExtensions;

namespace MyPhotoBooth.UnitTests.Features.Photos.Commands;

public class UploadPhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IImageProcessingService> _imageProcessingMock;
    private readonly UploadPhotoCommandHandler _handler;
    private readonly Mock<IConfiguration> _configurationMock;

    public UploadPhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _imageProcessingMock = new Mock<IImageProcessingService>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock
            .Setup(x => x["StorageSettings:MaxFileSizeMB"])
            .Returns("50");

        _handler = new UploadPhotoCommandHandler(
            _photoRepositoryMock.Object,
            _fileStorageMock.Object,
            _imageProcessingMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task Handle_ValidFile_ReturnsSuccess()
    {
        // Arrange
        var file = new FormFile(new MemoryStream(), 0, 1024, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var command = new UploadPhotoCommand(file, "Test photo");

        _imageProcessingMock
            .Setup(x => x.IsValidImageFile(It.IsAny<Stream>(), It.IsAny<string>()))
            .Returns(true);

        _imageProcessingMock
            .Setup(x => x.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessedImageResult
            {
                OriginalStream = new MemoryStream(),
                ThumbnailStream = new MemoryStream(),
                Width = 1920,
                Height = 1080,
                ExifDataJson = null
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBeEmpty();

        _photoRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Photo>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FileTooLarge_ReturnsFailure()
    {
        // Arrange
        var largeFileSize = 51 * 1024 * 1024; // 51MB
        var file = new FormFile(new MemoryStream(), 0, largeFileSize, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var command = new UploadPhotoCommand(file, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("File size exceeds limit");

        _imageProcessingMock.Verify(
            x => x.ProcessImageAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidImageFormat_ReturnsFailure()
    {
        // Arrange
        var file = new FormFile(new MemoryStream(), 0, 1024, "file", "test.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var command = new UploadPhotoCommand(file, null);

        _imageProcessingMock
            .Setup(x => x.IsValidImageFile(It.IsAny<Stream>(), It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(Errors.Photos.InvalidFile);
    }
}
```

#### Validator Tests

```csharp
// tests/MyPhotoBooth.UnitTests/Features/Photos/Validators/UploadPhotoCommandValidatorTests.cs
using FluentValidation.TestHelper;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class UploadPhotoCommandValidatorTests
{
    private readonly UploadPhotoCommandValidator _validator;

    public UploadPhotoCommandValidatorTests()
    {
        _validator = new UploadPhotoCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Null()
    {
        // Arrange
        var command = new UploadPhotoCommand(null!, "description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public void Should_Not_Have_Error_When_File_Is_Valid()
    {
        // Arrange
        var file = new FormFile(new MemoryStream(), 0, 1024, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var command = new UploadPhotoCommand(file, "description");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Description_Too_Long()
    {
        // Arrange
        var file = new FormFile(new MemoryStream(), 0, 1024, "file", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        var longDescription = new string('A', 1001);
        var command = new UploadPhotoCommand(file, longDescription);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description too long");
    }
}
```

#### Test Helpers

```csharp
// tests/MyPhotoBooth.UnitTests/Common/TestHelpers.cs
using Moq;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.UnitTests.Common;

public static class TestHelpers
{
    public static Mock<IFileStorageService> CreateFileStorageMock()
    {
        var mock = new Mock<IFileStorageService>();
        mock.Setup(x => x.BuildStoragePath(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string userId, string filename, bool isThumbnail)
                => $"/storage/{userId}/{(isThumbnail ? "thumbs" : "original")}/{filename}");
        return mock;
    }

    public static Stream CreateTestImageStream(int sizeInBytes = 1024)
    {
        var stream = new MemoryStream(new byte[sizeInBytes]);
        return stream;
    }

    public static FormFile CreateTestFormFile(string fileName = "test.jpg", long size = 1024)
    {
        return new FormFile(CreateTestImageStream((int)size), 0, size, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }
}
```

### Integration Testing

#### PostgreSQL Test Container

```csharp
// tests/MyPhotoBooth.IntegrationTests/Fixtures/PostgreSqlFixture.cs
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;

namespace MyPhotoBooth.IntegrationTests.Fixtures;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly TestcontainersContainer _container;
    private readonly string _database = "myphotobooth_test";
    private readonly string _username = "postgres";
    private readonly string _password = "postgres";

    public string ConnectionString { get; private set; } = string.Empty;

    public PostgreSqlFixture()
    {
        _container = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage("postgres:16")
            .WithEnvironment("POSTGRES_DB", _database)
            .WithEnvironment("POSTGRES_USER", _username)
            .WithEnvironment("POSTGRES_PASSWORD", _password)
            .WithPortBinding(5432, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(5432);

        ConnectionString = $"Host={host};Port={port};Database={_database};Username={_username};Password={_password}";

        // Run migrations
        // You can use a separate migration runner or the API's migration endpoint
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
```

#### WebApplicationFactory

```csharp
// tests/MyPhotoBooth.IntegrationTests/Fixtures/ApiWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyPhotoBooth.Infrastructure.Persistence;

namespace MyPhotoBooth.IntegrationTests.Fixtures;

public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ApiWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the actual AppDbContext
            services.RemoveAll<AppDbContext>();

            // Add test AppDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
                options.EnableSensitiveDataLogging(); // For debugging
                options.EnableDetailedErrors();
            });

            // Remove email service for tests
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, MockEmailService>();
        });
    }
}

// Mock email service that doesn't send real emails
public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        // Log instead of sending
        return Task.CompletedTask;
    }
}
```

#### Integration Test Example

```csharp
// tests/MyPhotoBooth.IntegrationTests/Features/Photos/PhotosEndpointTests.cs
using FluentAssertions;
using Xunit;
using System.Net.Http.Json;
using MyPhotoBooth.IntegrationTests.Fixtures;
using MyPhotoBooth.Application.Common.DTOs;

namespace MyPhotoBooth.IntegrationTests.Features.Photos;

[Collection("Database Collection")]
public class PhotosEndpointTests : IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly PostgreSqlFixture _dbFixture;
    private readonly ApiWebApplicationFactory _factory;
    private string _authToken = string.Empty;

    public PhotosEndpointTests(PostgreSqlFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = new ApiWebApplicationFactory(dbFixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Seed test data
        await SeedTestData();

        // Get auth token
        _authToken = await GetAuthToken();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task UploadPhoto_ValidFile_ReturnsCreated()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");

        using var content = new MultipartFormDataContent();
        using var fileStream = File.OpenRead("TestData/test-image.jpg");
        content.Add(new StreamContent(fileStream), "file", "test-image.jpg");
        content.Add(new StringContent("Test description"), "description");

        // Act
        var response = await _client.PostAsync("/api/photos", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PhotoUploadResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();
        result.OriginalFileName.Should().Be("test-image.jpg");
    }

    [Fact]
    public async Task UploadPhoto_NoFile_ReturnsBadRequest()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");

        using var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync("/api/photos", content);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPhoto_ValidId_ReturnsPhoto()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        var photoId = await CreateTestPhoto();

        // Act
        var response = await _client.GetAsync($"/api/photos/{photoId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PhotoDetailsResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(photoId);
    }

    private async Task SeedTestData()
    {
        // Use a service scope to add test data
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Add test user, etc.
        // ...
        await dbContext.SaveChangesAsync();
    }

    private async Task<string> GetAuthToken()
    {
        // Register/login and return JWT token
        // ...
        return "test-token";
    }

    private async Task<Guid> CreateTestPhoto()
    {
        // Create and return photo ID
        // ...
        return Guid.NewGuid();
    }
}

// Collection fixture for shared database context
[CollectionDefinition("Database Collection")]
public class DatabaseCollection : ICollectionFixture<PostgreSqlFixture>
{
}
```

### Test Database Strategy Decision Tree

```
Start
  │
  ├─> Need to test EF Core queries?
  │   ├─ Yes ─> Use Testcontainers (Integration Tests)
  │   └─ No ─> Use Moq (Unit Tests)
  │
  ├─> Testing business logic?
  │   └─> Use Moq (Unit Tests)
  │
  └─> Testing HTTP endpoints?
      └─> Use Testcontainers + WebApplicationFactory (Integration Tests)
```

---

## Migration Plan

### Phase 1: Foundation (1-2 days)

**Goal**: Set up infrastructure without changing existing code

1. Install packages
2. Create test projects
3. Set up folder structure
4. Create base types (ICommand, IQuery, etc.)
5. Create pipeline behaviors
6. Register services in DI
7. Configure test infrastructure

**No breaking changes** - existing services continue to work

### Phase 2: Proof of Concept (1-2 days)

**Goal**: Migrate one feature completely as reference

1. Choose Photos feature (it's representative)
2. Create Commands/Queries
3. Create Validators
4. Create Handlers
5. Write unit tests
6. Write integration tests
7. Update controller to use MediatR
8. Run full test suite
9. Keep old service temporarily (canary)

### Phase 3: Gradual Migration (3-5 days)

**Goal**: Migrate remaining features one by one

**Migration Order** (simple to complex):
1. Tags (simple CRUD)
2. Albums (medium complexity)
3. ShareLinks (medium complexity)
4. Photos (already done in PoC)
5. Auth (most complex, keep for last)

**Each Feature Migration Steps**:
1. Create Commands/Queries
2. Create Validators + Tests
3. Create Handlers + Tests
4. Update Controller
5. Test manually
6. Remove old service

### Phase 4: Cleanup (1 day)

1. Remove all old service interfaces and implementations
2. Remove unused DI registrations
3. Update documentation
4. Run full test suite
5. Performance testing

### Feature Migration Checklist

For each feature (Photos, Albums, Tags, ShareLinks, Auth):

- [ ] Commands created
- [ ] Queries created
- [ ] Validators created
- [ ] Validator tests passing
- [ ] Handlers created
- [ ] Handler tests passing
- [ ] Controller updated
- [ ] Integration tests passing
- [ ] Old service removed
- [ ] Documentation updated

---

## Code Examples

### Complete Example: Upload Photo

#### Command

```csharp
// Application/Features/Photos/Commands/UploadPhotoCommand.cs
using MediatR;
using Microsoft.AspNetCore.Http;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Commands;

public record UploadPhotoCommand(
    IFormFile File,
    string? Description
) : ICommand<PhotoUploadResponse>;
```

#### Validator

```csharp
// Application/Features/Photos/Validators/UploadPhotoCommandValidator.cs
using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Photos.Commands;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class UploadPhotoCommandValidator : AbstractValidator<UploadPhotoCommand>
{
    public UploadPhotoCommandValidator()
    {
        RuleFor(x => x.File).ImageFile(maxSizeMB: 50);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
```

#### Handler

```csharp
// Application/Features/Photos/Handlers/UploadPhotoCommandHandler.cs
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using System.Security.Claims;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class UploadPhotoCommandHandler : ICommandHandler<UploadPhotoCommand, PhotoUploadResponse>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UploadPhotoCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UploadPhotoCommandHandler(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        IImageProcessingService imageProcessingService,
        IConfiguration configuration,
        ILogger<UploadPhotoCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _imageProcessingService = imageProcessingService;
        _configuration = configuration;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<PhotoUploadResponse>> Handle(
        UploadPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(Errors.General.Unauthorized);

        var maxFileSizeMB = int.Parse(_configuration["StorageSettings:MaxFileSizeMB"] ?? "50");
        if (request.File.Length > maxFileSizeMB * 1024 * 1024)
        {
            return Result.Failure<PhotoUploadResponse>(
                $"{Errors.Photos.FileTooLarge} (max {maxFileSizeMB}MB)");
        }

        using var stream = request.File.OpenReadStream();
        if (!_imageProcessingService.IsValidImageFile(stream, request.File.ContentType))
        {
            return Result.Failure<PhotoUploadResponse>(Errors.Photos.InvalidFile);
        }

        var storageKey = Guid.NewGuid().ToString();
        var storedFileName = $"{storageKey}.jpg";

        stream.Position = 0;
        var processed = await _imageProcessingService.ProcessImageAsync(stream, cancellationToken);

        var userIdPath = userId;
        var originalPath = _fileStorageService.BuildStoragePath(userIdPath, storedFileName, false);
        var thumbnailPath = _fileStorageService.BuildStoragePath(userIdPath, storedFileName, true);

        await _fileStorageService.SaveFileAsync(processed.OriginalStream, originalPath, cancellationToken);
        await _fileStorageService.SaveFileAsync(processed.ThumbnailStream, thumbnailPath, cancellationToken);

        processed.OriginalStream.Dispose();
        processed.ThumbnailStream.Dispose();

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = request.File.FileName,
            StorageKey = storageKey,
            FilePath = originalPath,
            ThumbnailPath = thumbnailPath,
            FileSize = request.File.Length,
            ContentType = "image/jpeg",
            Width = processed.Width,
            Height = processed.Height,
            UploadedAt = DateTime.UtcNow,
            Description = request.Description,
            UserId = userId,
            ExifDataJson = processed.ExifDataJson
        };

        await _photoRepository.AddAsync(photo, cancellationToken);

        _logger.LogInformation("Photo uploaded: {PhotoId} for user {UserId}", photo.Id, userId);

        return Result.Success(new PhotoUploadResponse
        {
            Id = photo.Id,
            OriginalFileName = photo.OriginalFileName,
            FileSize = photo.FileSize,
            Width = processed.Width,
            Height = processed.Height,
            UploadedAt = photo.UploadedAt,
            Description = photo.Description
        });
    }
}
```

#### Controller (updated)

```csharp
// API/Controllers/PhotosController.cs
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ApiControllerBase
{
    private readonly ISender _mediator;

    public PhotosController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(
        IFormFile file,
        [FromForm] string? description,
        CancellationToken cancellationToken)
    {
        var command = new UploadPhotoCommand(file, description);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhoto(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPhotoQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet]
    public async Task<IActionResult> ListPhotos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new ListPhotosQuery(page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePhoto(
        Guid id,
        [FromBody] UpdatePhotoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePhotoCommand(id, request.Description);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePhotoCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        return result.ToHttpResponse();
    }

    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetPhotoFile(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPhotoFileQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.IsSuccess
            ? File(result.Value.Stream, result.Value.ContentType, result.Value.FileName)
            : result.ToHttpResponse();
    }

    [HttpGet("{id}/thumbnail")]
    public async Task<IActionResult> GetPhotoThumbnail(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPhotoThumbnailQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return result.IsSuccess
            ? File(result.Value, "image/jpeg")
            : result.ToHttpResponse();
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPhotoTimelineQuery(year, month, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return result.ToHttpResponse();
    }
}
```

### Query Example

```csharp
// Application/Features/Photos/Queries/ListPhotosQuery.cs
using MediatR;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record ListPhotosQuery(
    int Page = 1,
    int PageSize = 50
) : IQuery<PaginatedResponse<PhotoListResponse>>;

// Handler
public class ListPhotosQueryHandler : IQueryHandler<ListPhotosQuery, PaginatedResponse<PhotoListResponse>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ListPhotosQueryHandler(
        IPhotoRepository photoRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _photoRepository = photoRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<PaginatedResponse<PhotoListResponse>>> Handle(
        ListPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var skip = (request.Page - 1) * request.PageSize;

        var photos = await _photoRepository.GetByUserIdAsync(
            userId, skip, request.PageSize, cancellationToken);
        var totalCount = await _photoRepository.GetCountByUserIdAsync(userId, cancellationToken);

        var photoList = photos.Select(p => new PhotoListResponse
        {
            Id = p.Id,
            OriginalFileName = p.OriginalFileName,
            Width = p.Width,
            Height = p.Height,
            CapturedAt = p.CapturedAt,
            UploadedAt = p.UploadedAt,
            ThumbnailPath = p.ThumbnailPath
        }).ToList();

        return Result.Success(new PaginatedResponse<PhotoListResponse>
        {
            Items = photoList,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        });
    }

    private string GetUserId()
    {
        return _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(Errors.General.Unauthorized);
    }
}
```

---

## Effort Estimation

| Phase | Tasks | Effort | Notes |
|-------|-------|--------|-------|
| **Phase 1: Foundation** | Package install, test projects setup, folder structure, base types, pipeline behaviors, DI registration | 1-2 days | No breaking changes |
| **Phase 2: PoC** | Photos feature migration (C/Q/V/H + tests) | 1-2 days | Reference implementation |
| **Phase 3: Migration** | Tags (1d), Albums (1.5d), ShareLinks (1.5d), Auth (2d) | 5-6 days | One feature at a time |
| **Phase 4: Cleanup** | Remove old code, update docs, final testing | 1 day | |
| **Buffer** | Unexpected issues, refinements | 1-2 days | |
| **Total** | | **9-13 days** | ~2 weeks |

### By Activity Breakdown

| Activity | Effort |
|----------|--------|
| Commands/Queries creation | 2-3 days |
| Validators + tests | 2-3 days |
| Handlers + tests | 3-4 days |
| Integration tests | 1-2 days |
| Controller updates | 1 day |
| Infrastructure setup | 1 day |
| Cleanup & docs | 0.5 day |
| **Total** | **10.5-14.5 days** |

---

## Potential Pitfalls

### 1. HttpContext Access in Handlers

**Problem**: Handlers need user ID from HttpContext

**Solution**: Inject `IHttpContextAccessor` (already in DI)

```csharp
public class UploadPhotoCommandHandler : ICommandHandler<UploadPhotoCommand, PhotoUploadResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UploadPhotoCommandHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<PhotoUploadResponse>> Handle(...)
    {
        var userId = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
    }
}
```

### 2. Validation Pipeline Behavior Generic Type Mismatch

**Problem**: `ValidationBehavior` expects `Result<T>` but handlers return `CSharpFunctionalExtensions.Result<T>`

**Solution**: Use reflection or create explicit behavior for CSharpFunctionalExtensions

```csharp
// In ValidationBehavior.cs
var failureMethod = resultType.GetMethod("Failure", new[] { typeof(string) });
if (failureMethod != null)
{
    return (TResponse)failureMethod.Invoke(null, new object[] { errorMessage })!;
}
```

### 3. Transaction Behavior with MultipleDbContexts

**Problem**: If you add another DbContext later, transaction won't span both

**Solution**: Use `IDbContextTransaction` and explicit transaction scope for complex scenarios

### 4. Test Database Contention

**Problem**: Tests running in parallel modify same database

**Solutions**:
- Run tests sequentially (xUnit default)
- Use unique database per test class
- Use rollback transactions in tests

```csharp
// In test setup
await _dbContext.Database.BeginTransactionAsync();

// In test cleanup
await _dbContext.RollbackTransactionAsync();
```

### 5. File Upload Testing

**Problem**: Testing file uploads in handlers requires IFormFile

**Solution**: Create test helper to build FormFile from MemoryStream

```csharp
public static FormFile CreateTestFormFile(string fileName, long size)
{
    return new FormFile(new MemoryStream(new byte[size]), 0, size, "file", fileName)
    {
        Headers = new HeaderDictionary(),
        ContentType = "image/jpeg"
    };
}
```

### 6. MediatR Handler Not Found

**Problem**: Handler not registered despite being in correct assembly

**Solution**: Ensure handler is in assembly scanned by MediatR

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(UploadPhotoCommandHandler).Assembly);
    // Or use the Assembly that contains your handlers
});
```

### 7. Pipeline Behavior Order

**Problem**: Validation runs after transaction begins

**Solution**: Register behaviors in correct order

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));     // 1st
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));        // 2nd
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));    // 3rd
```

### 8. IFormFile Serialization

**Problem**: IFormFile can't be serialized for distributed tracing

**Solution**: Extract needed data before sending to mediator

```csharp
// Controller
using var stream = file.OpenReadStream();
var command = new UploadPhotoCommand(
    stream,
    file.FileName,
    file.ContentType,
    file.Length,
    description);
```

### 9. Testcontainers Performance

**Problem**: Tests are slow with PostgreSQL container

**Solutions**:
- Use shared fixture (one container for all tests)
- Use in-memory for unit tests
- Only use Testcontainers for integration tests

### 10. Migrating Without Breaking Changes

**Problem**: Need to migrate incrementally

**Solution**: Run both old and new in parallel

```csharp
// Controller
[HttpPost]
public async Task<IActionResult> UploadPhoto(...)
{
    // Use new MediatR handler
    var command = new UploadPhotoCommand(file, description);
    var result = await _mediator.Send(command, cancellationToken);

    // Old service still available for rollback if needed
    // var result = await _photoService.UploadPhotoAsync(...);

    return result.ToHttpResponse();
}
```

---

## Appendix

### A. Complete File List for Migration

```
Application/
├── Common/
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   ├── LoggingBehavior.cs
│   │   └── TransactionBehavior.cs
│   ├── Requests/
│   │   ├── IRequest.cs
│   │   ├── ICommand.cs
│   │   └── IQuery.cs
│   ├── Results/
│   │   └── ResultExtensions.cs
│   └── Validators/
│       └── SharedValidators.cs
└── Features/
    ├── Photos/
    │   ├── Commands/
    │   │   ├── UploadPhotoCommand.cs
    │   │   ├── UpdatePhotoCommand.cs
    │   │   ├── DeletePhotoCommand.cs
    │   │   └── AddPhotoToAlbumCommand.cs
    │   ├── Queries/
    │   │   ├── GetPhotoQuery.cs
    │   │   ├── ListPhotosQuery.cs
    │   │   ├── GetPhotoTimelineQuery.cs
    │   │   ├── GetPhotoFileQuery.cs
    │   │   └── GetPhotoThumbnailQuery.cs
    │   ├── Validators/
    │   │   ├── UploadPhotoCommandValidator.cs
    │   │   ├── UpdatePhotoCommandValidator.cs
    │   │   ├── ListPhotosQueryValidator.cs
    │   │   └── GetPhotoTimelineQueryValidator.cs
    │   └── Handlers/
    │       ├── UploadPhotoCommandHandler.cs
    │       ├── UpdatePhotoCommandHandler.cs
    │       ├── DeletePhotoCommandHandler.cs
    │       ├── GetPhotoQueryHandler.cs
    │       ├── ListPhotosQueryHandler.cs
    │       ├── GetPhotoTimelineQueryHandler.cs
    │       ├── GetPhotoFileQueryHandler.cs
    │       └── GetPhotoThumbnailQueryHandler.cs
    ├── Albums/
    │   └── (similar structure)
    ├── Tags/
    │   └── (similar structure)
    ├── Auth/
    │   └── (similar structure)
    └── ShareLinks/
        └── (similar structure)
```

### B. References

- [MediatR GitHub](https://github.com/LuckyPennySoftware/MediatR)
- [FluentValidation GitHub](https://github.com/FluentValidation/FluentValidation)
- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
- [Testcontainers for .NET](https://github.com/testcontainers/testcontainers-dotnet)
- [Jimmy Bogard: MediatR Pipeline Examples](https://jimmybogard.com/meditr-pipeline-examples/)
- [Steve Smith: Clean Architecture](https://github.com/ardalis/CleanArchitecture)
- [Microsoft: Testing with WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

## Open Questions

1. **Test Database Strategy Confirmation**: Proceed with In-Memory + Moq for unit tests? (Recommended)
2. **Testcontainers vs Real Test DB**: Use Testcontainers for integration tests? (Recommended)
3. **Test Parallel Execution**: Configure tests to run sequentially (default) or enable parallel with unique DBs?
4. **HttpContext in Handlers**: Use IHttpContextAccessor or pass userId as part of command?
5. **Transaction Scope**: Current behavior handles single DbContext. Need distributed transactions?

---

## Decision Log

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Test DB for Unit Tests | In-Memory + Moq | Speed, simplicity, adequate for testing handlers |
| Test DB for Integration Tests | Testcontainers PostgreSQL | Production-like, CI/CD friendly |
| Validator Location | Co-located with requests | Easier to find, better cohesion |
| Feature Migration Order | Tags → Albums → ShareLinks → Photos → Auth | Simple to complex, Auth last due to security |
| HttpContext Access | IHttpContextAccessor in handlers | Standard ASP.NET Core pattern |
| Transaction Scope | Single DbContext per handler | Simplicity, adequate for current needs |

---

## Next Steps

1. Review this plan with team
2. Confirm open questions
3. Begin Phase 1: Foundation
4. Create PoC with Photos feature
5. Validate approach with stakeholders
6. Proceed with full migration

---

**Document Version**: 1.0
**Author**: Claude Code Planning Assistant
**Status**: Ready for Review
**Target Start Date**: TBD
**Target Completion Date**: TBD
