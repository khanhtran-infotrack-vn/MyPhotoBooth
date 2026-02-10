# Code Standards - MyPhotoBooth v1.3.0

## Table of Contents

1. [Overview](#overview)
2. [C# Coding Standards](#c-coding-standards)
3. [CQRS Pattern Standards](#cqrs-pattern-standards)
4. [Validation Standards](#validation-standards)
5. [Testing Standards](#testing-standards)
6. [Frontend Standards](#frontend-standards)
7. [Naming Conventions](#naming-conventions)
8. [Code Review Guidelines](#code-review-guidelines)

## Overview

This document outlines the coding standards and conventions for MyPhotoBooth project. Following these standards ensures code quality, maintainability, and consistency across the codebase.

## C# Coding Standards

### General Guidelines

1. **Use async/await for all I/O operations**:
   ```csharp
   // Good
   public async Task<Photo?> GetByIdAsync(Guid id, CancellationToken ct)
   {
       return await _context.Photos.FindAsync(new object[] { id }, ct);
   }

   // Bad - synchronous I/O
   public Photo? GetById(Guid id)
   {
       return _context.Photos.Find(id);
   }
   ```

2. **Always pass CancellationToken to async methods**:
   ```csharp
   // Good
   public async Task<Result<PhotoDto>> Handle(
       CreatePhotoCommand request,
       CancellationToken cancellationToken)
   {
       var photo = await _repository.GetByIdAsync(request.Id, cancellationToken);
   }

   // Bad - no cancellation token
   public async Task<Result<PhotoDto>> Handle(CreatePhotoCommand request)
   {
       var photo = await _repository.GetByIdAsync(request.Id);
   }
   ```

3. **Use pattern matching for type checking**:
   ```csharp
   // Good
   if (entity is Photo photo)
   {
       return photo.FileName;
   }

   // Bad
   if (entity is Photo)
   {
       var photo = (Photo)entity;
       return photo.FileName;
   }
   ```

4. **Prefer null-coalescing operators**:
   ```csharp
   // Good
   var title = photo.Title ?? "Untitled";
   var count = photos?.Count ?? 0;

   // Bad
   var title = photo.Title != null ? photo.Title : "Untitled";
   ```

5. **Use string interpolation over string concatenation**:
   ```csharp
   // Good
   var message = $"Photo {photo.Id} uploaded by user {userId}";

   // Bad
   var message = "Photo " + photo.Id + " uploaded by user " + userId;
   ```

6. **Use expression-bodied members for simple methods**:
   ```csharp
   // Good
   public string FullName => $"{FirstName} {LastName}";

   // Good for multi-line
   public bool IsValid() =>
       !string.IsNullOrEmpty(Email) &&
       !string.IsNullOrEmpty(Password);

   // Bad - unnecessary braces for single expression
   public string FullName()
   {
       return $"{FirstName} {LastName}";
   }
   ```

### Exception Handling

1. **Use Result<T> pattern for expected errors**:
   ```csharp
   // Good - use Result pattern
   if (photo == null)
   {
       return Result.Failure(Errors.Photos.NotFound);
   }

   // Bad - throw exception for expected case
   if (photo == null)
   {
       throw new NotFoundException("Photo not found");
   }
   ```

2. **Use specific exception types**:
   ```csharp
   // Good
   throw new ArgumentException("Invalid photo ID", nameof(photoId));

   // Bad
   throw new Exception("Invalid photo ID");
   ```

3. **Include context in exceptions**:
   ```csharp
   // Good
   _logger.LogError(ex, "Failed to process photo {PhotoId}", photoId);
   throw new InvalidOperationException(
       $"Failed to process photo {photoId}. See inner exception for details.",
       ex);

   // Bad - no context
   throw new InvalidOperationException("Failed to process photo");
   ```

### LINQ Guidelines

1. **Use method syntax for LINQ**:
   ```csharp
   // Good
   var activePhotos = photos
       .Where(p => !p.IsDeleted)
       .OrderBy(p => p.CreatedAt)
       .ToList();

   // Acceptable for complex queries - query syntax
   var activePhotos = from p in photos
                      where !p.IsDeleted
                      orderby p.CreatedAt
                      select p;
   ```

2. **Avoid enumerating IEnumerable multiple times**:
   ```csharp
   // Good
   var photoList = photos.ToList();
   var count = photoList.Count;
   var first = photoList.FirstOrDefault();

   // Bad - enumerates twice
   var count = photos.Count();
   var first = photos.FirstOrDefault();
   ```

3. **Use Any() instead of Count() > 0**:
   ```csharp
   // Good
   if (photos.Any())
   {
       // process photos
   }

   // Bad - less efficient
   if (photos.Count() > 0)
   {
       // process photos
   }
   ```

## CQRS Pattern Standards

### Command Definition

```csharp
// Command record - immutable
public record CreatePhotoCommand(
    IFormFile File,
    string? Title,
    string? Description
) : ICommand<PhotoDto>;

// Always use records for commands/queries
// Always implement ICommand<T> or IQuery<T>
// Property names use PascalCase
```

### Command Handler

```csharp
public class CreatePhotoCommandHandler : ICommandHandler<CreatePhotoCommand, PhotoDto>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IUserContext _userContext;

    public CreatePhotoCommandHandler(
        IPhotoRepository photoRepository,
        IImageProcessingService imageProcessingService,
        IUserContext userContext)
    {
        _photoRepository = photoRepository;
        _imageProcessingService = imageProcessingService;
        _userContext = userContext;
    }

    public async Task<Result<PhotoDto>> Handle(
        CreatePhotoCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate business rules
        // 2. Perform operations
        // 3. Return Result.Success() or Result.Failure()

        if (request.File == null)
        {
            return Result.Failure(Errors.Photos.NoFileUploaded);
        }

        var photo = // ... create photo

        await _photoRepository.AddAsync(photo, cancellationToken);
        await _photoRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(photo.ToDto());
    }
}

// Handler naming: {CommandName}Handler
// Implement ICommandHandler or IQueryHandler
// Dependencies injected via constructor
// Always return Result<T>
```

### Query Definition

```csharp
// Query record - immutable
public record GetPhotosQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? AlbumId = null,
    Guid? TagId = null
) : IQuery<PaginatedResult<PhotoDto>>;

// Provide default values for optional parameters
// Use nullable types for optional parameters
```

### Query Handler

```csharp
public class GetPhotosQueryHandler : IQueryHandler<GetPhotosQuery, PaginatedResult<PhotoDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IUserContext _userContext;

    public GetPhotosQueryHandler(
        IPhotoRepository photoRepository,
        IUserContext userContext)
    {
        _photoRepository = photoRepository;
        _userContext = userContext;
    }

    public async Task<Result<PaginatedResult<PhotoDto>>> Handle(
        GetPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        var (photos, totalCount) = await _photoRepository.GetPagedAsync(
            userId,
            request.Page,
            request.PageSize,
            request.Search,
            request.AlbumId,
            request.TagId,
            cancellationToken);

        var photoDtos = photos.Select(p => p.ToDto()).ToList();

        var paginatedResult = new PaginatedResult<PhotoDto>(
            photoDtos,
            totalCount,
            request.Page,
            request.PageSize);

        return Result.Success(paginatedResult);
    }
}

// Query handlers should not modify state
// Use ToDto() to transform entities to DTOs
```

### Controller Standards

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

    [HttpGet("{id}")]
    public async Task<ActionResult<PhotoDto>> GetPhoto(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetPhotoQuery(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}

// Controllers are thin wrappers
// Use ISender to dispatch commands/queries
// Return appropriate HTTP status codes
// Handle Result<T> properly
```

## Validation Standards

### Validator Definition

```csharp
public class CreatePhotoCommandValidator : AbstractValidator<CreatePhotoCommand>
{
    public CreatePhotoCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage(Errors.Photos.NoFileUploaded)
            .Must(BeValidImageFile).WithMessage(Errors.Photos.InvalidFile)
            .Must(BeWithinSizeLimit).WithMessage(Errors.Photos.FileTooLarge);

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
    }

    private bool BeValidImageFile(IFormFile file)
    {
        if (file == null) return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return allowedExtensions.Contains(extension);
    }

    private bool BeWithinSizeLimit(IFormFile file)
    {
        return file.Length <= 50 * 1024 * 1024; // 50MB
    }
}

// Validator naming: {CommandName}Validator
// Inherit from AbstractValidator<T>
// Use static error constants from Errors.cs
// Create private methods for complex validation logic
```

### Custom Validators

```csharp
public class EmailValidator<T> : PropertyValidator<T, string>
{
    public override string Name => "EmailValidator";

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "'{PropertyName}' is not a valid email address.";

    public override bool IsValid(ValidationContext<T> context, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true; // Use Required validator for null checks

        // Simple email validation
        return value.Contains('@') && value.Contains('.');
    }
}

// Custom validators should be reusable
// Use generic type parameter for flexibility
// Return true for null values (use Required validator separately)
```

### Using Custom Validators

```csharp
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("'Email' must not be empty.")
            .SetValidator(new EmailValidator<RegisterCommand>());

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("'Password' must not be empty.")
            .SetValidator(new StrongPasswordValidator<RegisterCommand>());
    }
}
```

## Testing Standards

### Unit Test Structure

```csharp
public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_IsNull()
    {
        // Arrange
        var command = new LoginCommand(null!, "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("'Email' must not be empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Have_Error_When_Password_IsInvalid(string? password)
    {
        // Arrange
        var command = new LoginCommand("test@example.com", password!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

// Test naming: Should_{ExpectedBehavior}_When_{StateUnderTest}
// Use AAA pattern (Arrange, Act, Assert)
// Use Theory for multiple test cases
// Keep tests simple and focused
```

### Integration Test Structure

```csharp
[Collection("Database")]
public class AuthEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AuthEndpointTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeEmpty();
    }
}

// Use Collection fixtures for shared database
// Use Class fixtures for shared test factory
// Test realistic scenarios
// Clean up after tests
```

## Frontend Standards

### TypeScript Guidelines

1. **Use TypeScript for all new code**:
   ```typescript
   // Good - with types
   interface Photo {
     id: string;
     title: string;
     createdAt: Date;
   }

   const getPhoto = async (id: string): Promise<Photo> => {
     // ...
   };

   // Bad - no types
   const getPhoto = async (id) => {
     // ...
   };
   ```

2. **Use interfaces for data shapes**:
   ```typescript
   // Good
   interface PhotoDto {
     id: string;
     title: string;
     description: string | null;
     createdAt: string;
   }

   // Bad - use type for primitives
   type Photo = {
     id: string;
     title: string;
   };
   ```

3. **Use union types for variants**:
   ```typescript
   // Good
   type ThemeMode = 'light' | 'dark' | 'system';

   // Bad
   type ThemeMode = string;
   ```

### React Component Guidelines

1. **Use function components with hooks**:
   ```typescript
   // Good
   export function PhotoGallery({ photos }: PhotoGalleryProps) {
     const [selectedPhoto, setSelectedPhoto] = useState<Photo | null>(null);

     return (
       <div>
         {photos.map(photo => (
           <PhotoCard key={photo.id} photo={photo} />
         ))}
       </div>
     );
   }

   // Bad - class component
   export class PhotoGallery extends Component {
     // ...
   }
   ```

2. **Use TypeScript for props**:
   ```typescript
   // Good
   interface PhotoCardProps {
     photo: Photo;
     onSelect: (photo: Photo) => void;
   }

   export function PhotoCard({ photo, onSelect }: PhotoCardProps) {
     // ...
   }

   // Bad - no types
   export function PhotoCard({ photo, onSelect }) {
     // ...
   }
   ```

3. **Keep components small and focused**:
   ```typescript
   // Good - focused component
   export function ThemeToggle() {
     const { themeMode, setThemeMode } = useUIStore();

     return (
       <button onClick={() => setThemeMode('light')}>
         Light
       </button>
     );
   }

   // Bad - component doing too much
   export function ThemeToggle() {
     const [theme, setTheme] = useState();
     const [isOpen, setIsOpen] = useState();
     const handleThemeChange = () => { /* complex logic */ };
     const fetchUserPreferences = () => { /* API call */ };
     // ...
   }
   ```

### State Management

1. **Use Zustand for client state**:
   ```typescript
   // Good - Zustand store
   interface UIState {
     themeMode: 'light' | 'dark' | 'system';
     setThemeMode: (mode: 'light' | 'dark' | 'system') => void;
   }

   export const useUIStore = create<UIState>((set) => ({
     themeMode: 'system',
     setThemeMode: (mode) => set({ themeMode: mode }),
   }));
   ```

2. **Use TanStack Query for server state**:
   ```typescript
   // Good - useQuery hook
   export function usePhotos() {
     return useQuery({
       queryKey: ['photos'],
       queryFn: () => api.get<Photo[]>('/photos'),
     });
   }

   // Good - useMutation hook
   export function useUploadPhoto() {
     const queryClient = useQueryClient();

     return useMutation({
       mutationFn: (file: File) => api.uploadPhoto(file),
       onSuccess: () => {
         queryClient.invalidateQueries({ queryKey: ['photos'] });
       },
     });
   }
   ```

## Naming Conventions

### C# Naming

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `PhotoService` |
| Interfaces | PascalCase with I prefix | `IPhotoService` |
| Methods | PascalCase | `GetPhotoById` |
| Properties | PascalCase | `PhotoTitle` |
| Fields | _camelCase with underscore | `_photoRepository` |
| Constants | PascalCase | `MaxFileSize` |
| Local variables | camelCase | `photoId` |
| Parameters | camelCase | `photoTitle` |
| Async methods | Async suffix | `GetPhotoAsync` |
| Test methods | Should_When_ pattern | `Should_Return_Error_When_Email_IsInvalid` |

### TypeScript Naming

| Type | Convention | Example |
|------|------------|---------|
| Components | PascalCase | `PhotoGallery` |
| Interfaces | PascalCase | `PhotoProps` |
| Types | PascalCase | `ThemeMode` |
| Functions | camelCase | `getPhotoById` |
| Constants | UPPER_SNAKE_CASE | `MAX_FILE_SIZE` |
| Variables | camelCase | `photoId` |
| Hooks | camelCase with 'use' prefix | `usePhotos` |
| Events | onPascalCase | `onPhotoSelect` |

### File Naming

| Type | Convention | Example |
|------|------------|---------|
| C# Classes | ClassName.cs | `PhotoService.cs` |
| C# Interfaces | IInterfaceName.cs | `IPhotoService.cs` |
| C# Tests | ClassNameTests.cs | `PhotoServiceTests.cs` |
| TypeScript Components | ComponentName.tsx | `PhotoGallery.tsx` |
| TypeScript Utilities | utilityName.ts | `formatDate.ts` |
| TypeScript Hooks | hookName.ts | `usePhotos.ts` |

## Code Review Guidelines

### Review Checklist

#### Functionality
- [ ] Does the code implement the requirements?
- [ ] Are edge cases handled?
- [ ] Is error handling appropriate?
- [ ] Are validations in place?

#### Code Quality
- [ ] Is the code readable and understandable?
- [ ] Are naming conventions followed?
- [ ] Is the code DRY (Don't Repeat Yourself)?
- [ ] Are there appropriate comments?

#### Architecture
- [ ] Does the code follow CQRS pattern?
- [ ] Are responsibilities properly separated?
- [ ] Are dependencies properly injected?
- [ ] Is the code testable?

#### Testing
- [ ] Are there unit tests?
- [ ] Are there integration tests?
- [ ] Do tests cover happy path?
- [ ] Do tests cover error cases?

#### Performance
- [ ] Are async operations properly awaited?
- [ ] Are there potential N+1 query problems?
- [ ] Is memory usage appropriate?
- [ ] Are there unnecessary computations?

#### Security
- [ ] Is user input validated?
- [ ] Are secrets properly stored?
- [ ] Is authentication/authorization correct?
- [ ] Are SQL injections prevented?

### Review Comments

1. **Be constructive**: Focus on improvement, not criticism
2. **Explain why**: Provide context for suggestions
3. **Be specific**: Point to exact lines or code blocks
4. **Ask questions**: Understand before suggesting changes
5. **Acknowledge good work**: Positive feedback matters

### Comment Examples

```markdown
# Good comment
Consider using async/await here for better performance. Synchronous I/O can block the thread pool.
See https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming

# Bad comment
This is wrong. Fix it.

# Good comment
Nice use of the Result pattern here! This makes error handling very clear.

# Good question
What happens if the photo file is null? Should we add a validator for this?
```

---

**Document Version**: 1.0
**Last Updated**: 2025-02-10
**MyPhotoBooth Version**: 1.3.0
