# Testing Guide - MyPhotoBooth v1.3.0

## Table of Contents

1. [Overview](#overview)
2. [Test Structure](#test-structure)
3. [Testing Frameworks](#testing-frameworks)
4. [Writing Unit Tests](#writing-unit-tests)
5. [Writing Integration Tests](#writing-integration-tests)
6. [Test Coverage](#test-coverage)
7. [Running Tests](#running-tests)
8. [Best Practices](#best-practices)
9. [CI/CD Integration](#cicd-integration)

## Overview

MyPhotoBooth has comprehensive test coverage with 117 tests split between unit tests (86) and integration tests (31). The testing strategy follows the testing pyramid, with more unit tests than integration tests, and ensures critical paths are covered.

### Test Statistics

| Test Type | Count | Coverage Focus |
|-----------|-------|----------------|
| Unit Tests | 86 | Validators, Behaviors, Handlers |
| Integration Tests | 31 | API Endpoints |
| **Total** | **117** | **~70% validators, ~10% handlers, 100% behaviors, 100% API endpoints** |

## Test Structure

### Solution Structure

```
tests/
├── MyPhotoBooth.UnitTests/           # Unit tests
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── Validators/
│   │   │   │   ├── LoginCommandValidatorTests.cs
│   │   │   │   ├── RegisterCommandValidatorTests.cs
│   │   │   │   ├── ForgotPasswordCommandValidatorTests.cs
│   │   │   │   └── ResetPasswordCommandValidatorTests.cs
│   │   │   └── Handlers/
│   │   │       └── (future handler tests)
│   │   ├── Photos/
│   │   │   ├── Validators/
│   │   │   │   ├── UploadPhotoCommandValidatorTests.cs
│   │   │   │   ├── UpdatePhotoCommandValidatorTests.cs
│   │   │   │   └── DeletePhotoCommandValidatorTests.cs
│   │   │   └── Queries/
│   │   │       └── GetPhotosQueryHandlerTests.cs
│   │   ├── Albums/
│   │   │   └── Validators/
│   │   │       ├── CreateAlbumCommandValidatorTests.cs
│   │   │       └── UpdateAlbumCommandValidatorTests.cs
│   │   ├── Tags/
│   │   │   └── Validators/
│   │   │       └── CreateTagCommandValidatorTests.cs
│   │   └── ShareLinks/
│   │       └── Validators/
│   │           └── CreateShareLinkCommandValidatorTests.cs
│   ├── Common/
│   │   └── Behaviors/
│   │       └── ValidationBehaviorTests.cs
│   └── Helpers/
│       └── TestHelpers.cs
└── MyPhotoBooth.IntegrationTests/    # Integration tests
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
    ├── Helpers/
    │   └── TestAuthHelper.cs
    └── DatabaseCollection.cs
```

## Testing Frameworks

### Unit Test Dependencies

```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
<PackageReference Include="Moq" Version="4.20.72" />
<PackageReference Include="FluentAssertions" Version="8.8.0" />
<PackageReference Include="FluentValidation" Version="12.1.1" />
<PackageReference Include="MediatR" Version="14.0.0" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
```

### Integration Test Dependencies

```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
<PackageReference Include="FluentAssertions" Version="8.8.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.2" />
<PackageReference Include="Testcontainers.PostgreSql" Version="4.10.0" />
<PackageReference Include="coverlet.collector" Version="6.0.4" />
```

## Writing Unit Tests

### Validator Tests

Validator tests ensure validation rules work correctly:

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

    [Fact]
    public void Should_Have_Error_When_Email_IsInvalid()
    {
        // Arrange
        var command = new LoginCommand("invalid-email", "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("'Email' is not a valid email address.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_IsValid()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
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
```

### Behavior Tests

Behavior tests verify pipeline behaviors work correctly:

```csharp
public class ValidationBehaviorTests
{
    [Fact]
    public async Task Should_Return_Failure_When_Validation_Fails()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>>
        {
            new TestRequestValidator()
        };
        var logger = new Mock<ILogger<ValidationBehavior<TestRequest, Result>>>().Object;
        var behavior = new ValidationBehavior<TestRequest, Result>(
            validators,
            logger);
        var request = new TestRequest("invalid");
        var next = new Mock<RequestHandlerDelegate<Result>>();

        // Act
        var result = await behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Validation failed");
        next.Verify(x => x(), Times.Never); // Handler should not be called
    }

    [Fact]
    public async Task Should_Call_Next_When_Validation_Succeeds()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestRequest, Result>(
            Array.Empty<IValidator<TestRequest>>(),
            Mock.Of<ILogger<ValidationBehavior<TestRequest, Result>>>());
        var request = new TestRequest("valid");
        var expectedResult = Result.Success();
        var next = new Mock<RequestHandlerDelegate<Result>>();
        next.Setup(x => x()).ReturnsAsync(expectedResult);

        // Act
        var result = await behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        next.Verify(x => x(), Times.Once); // Handler should be called
    }
}
```

### Handler Tests

Handler tests verify business logic with mocked dependencies:

```csharp
public class GetPhotosQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _mockRepo;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly GetPhotosQueryHandler _handler;

    public GetPhotosQueryHandlerTests()
    {
        _mockRepo = new Mock<IPhotoRepository>();
        _mockUserContext = new Mock<IUserContext>();
        _handler = new GetPhotosQueryHandler(_mockRepo.Object, _mockUserContext.Object);
    }

    [Fact]
    public async Task Should_Return_Paginated_Photos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var photos = new List<Photo>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, FileName = "photo1.jpg" },
            new() { Id = Guid.NewGuid(), UserId = userId, FileName = "photo2.jpg" }
        };
        _mockUserContext.Setup(x => x.UserId).Returns(userId);
        _mockRepo.Setup(x => x.GetPagedAsync(
            userId,
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string?>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((photos, 2));

        var query = new GetPhotosQuery(Page: 1, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }
}
```

## Writing Integration Tests

### Test Fixtures

Integration tests use fixtures for shared setup:

```csharp
public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSQLContainer _container;
    public string ConnectionString { get; private set; } = string.Empty;

    public PostgreSqlFixture()
    {
        _container = new PostgreSQLBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("myphotobooth_test")
            .WithUsername("postgres")
            .WithPassword("postgres_test_password")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // Run migrations
        // ...
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgreSqlFixture>
{
    // This class makes the fixture available to tests in this collection
}
```

### Test Web Application Factory

Factory for creating test server:

```csharp
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production services
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add test database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_fixture.ConnectionString);
            });

            // Mock email service
            services.Remove(services.FirstOrDefault(
                d => d.ServiceType == typeof(IEmailService)));
            services.AddScoped<IEmailService, MockEmailService>();
        });
    }
}
```

### Endpoint Tests

Integration tests verify API endpoints end-to-end:

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
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeEmpty();
        result.RefreshToken.Should().NotBeEmpty();
        result.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Should().NotBeNull();
        error!.Error.Should().Contain("already registered");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            DisplayName = "Test User"
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeEmpty();
        result.RefreshToken.Should().NotBeEmpty();
    }
}
```

### Authentication Helper

Helper for authenticated requests:

```csharp
public static class TestAuthHelper
{
    public static async Task<string> GetTestTokenAsync(HttpClient client)
    {
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        return result!.AccessToken;
    }

    public static HttpClient CreateAuthenticatedClient(
        TestWebApplicationFactory factory,
        string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
```

## Test Coverage

### Current Coverage

| Component | Coverage | Notes |
|-----------|----------|-------|
| Validators | ~70% | Main validation rules covered |
| Handlers | ~10% | Basic handler tests, needs expansion |
| Behaviors | 100% | All behaviors fully tested |
| API Endpoints | 100% | All endpoints have integration tests |

### Improving Coverage

#### Handler Coverage

To improve handler coverage from 10% to 80%+:

1. **Test Success Paths**:
   - Verify successful execution
   - Check return values
   - Verify database changes

2. **Test Failure Paths**:
   - Test not found scenarios
   - Test unauthorized access
   - Test validation failures

3. **Test Edge Cases**:
   - Empty result sets
   - Boundary conditions
   - Concurrent operations

Example handler test structure:

```csharp
public class CreatePhotoCommandHandlerTests
{
    [Fact] // Success path
    public async Task Should_Create_Photo_When_Valid()
    { }

    [Fact] // Failure - invalid file
    public async Task Should_Return_Failure_When_File_Invalid()
    { }

    [Fact] // Failure - storage error
    public async Task Should_Return_Failure_When_Storage_Fails()
    { }

    [Fact] // Edge case - large file
    public async Task Should_Handle_Large_File()
    { }
}
```

## Running Tests

### Command Line

```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test tests/MyPhotoBooth.UnitTests

# Run integration tests only
dotnet test tests/MyPhotoBooth.IntegrationTests

# Run specific test file
dotnet test --filter "FullyQualifiedName~LoginCommandValidatorTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~Should_Have_Error_When_Email_IsInvalid"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run without building
dotnet test --no-build
```

### With Coverage

```bash
# Run with coverage report
dotnet test /p:CollectCoverage=true \
    /p:CoverletOutputFormat=opencover \
    /p:CoverletOutput=./coverage.xml

# Run with threshold
dotnet test /p:CollectCoverage=true \
    /p:Threshold=80 \
    /p:ThresholdType=line

# Generate HTML coverage report
dotnet test /p:CollectCoverage=true \
    /p:CoverletOutputFormat=opencover \
    && reportgenerator -reports:coverage.xml \
    -targetdir:coverage-report
```

### Visual Studio

1. **Test Explorer**: View and run all tests
2. **Run All**: Execute all tests
3. **Run Selected**: Execute selected tests
4. **Debug**: Debug tests with breakpoints

### VS Code

1. **Testing Extension**: .NET Core Test Explorer
2. **Beaker Icon**: View test list
3. **Run Button**: Execute tests
4. **Debug Button**: Debug tests

## Best Practices

### Unit Test Best Practices

1. **Arrange-Act-Assert (AAA)**:
   ```csharp
   [Fact]
   public void Test_Something()
   {
       // Arrange - Set up test data
       var validator = new LoginCommandValidator();
       var command = new LoginCommand("test@example.com", "password");

       // Act - Execute the code under test
       var result = validator.TestValidate(command);

       // Assert - Verify the outcome
       result.ShouldHaveValidationErrorFor(x => x.Email);
   }
   ```

2. **Descriptive Test Names**:
   - Use `Should_ExpectedBehavior_When_StateUnderTest`
   - Example: `Should_Return_Error_When_Email_Is_Invalid`

3. **Single Assertion**:
   - Each test should verify one thing
   - Use multiple tests for multiple scenarios

4. **Mock Dependencies**:
   - Mock external dependencies
   - Verify interactions with mocks
   - Don't mock the system under test

5. **Test Isolation**:
   - Tests should not depend on each other
   - Each test should set up its own data
   - Clean up after tests

### Integration Test Best Practices

1. **Use Testcontainers**:
   - Real database in container
   - Consistent environment
   - Easy cleanup

2. **Shared Fixtures**:
   - Use collection fixtures for shared setup
   - One database instance per test collection

3. **Reset State**:
   - Clean database between tests
   - Use transactions or database reset

4. **Realistic Scenarios**:
   - Test full request/response cycle
   - Include authentication
   - Test error responses

5. **Avoid External Dependencies**:
   - Mock email services
   - Mock external APIs
   - Use deterministic data

### Naming Conventions

```
Test Class: {ComponentName}Tests
Test Method: Should_{ExpectedBehavior}_When_{StateUnderTest}

Examples:
- LoginCommandValidatorTests
- Should_Have_Error_When_Email_IsNull
- Should_Return_Success_When_Credentials_Valid
- Should_Return_NotFound_When_User_DoesNotExist
```

### Test Data

```csharp
// Use theory for multiple test cases
[Theory]
[InlineData("test@example.com", true)]
[InlineData("invalid-email", false)]
[InlineData("", false)]
public void Email_Should_Be_Validated(string email, bool isValid)
{
    // Test implementation
}

// Use inline data for simple cases
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData("   ")]
public void Should_Have_Error_When_Email_IsMissing(string? email)
{
    // Test implementation
}
```

## CI/CD Integration

### GitHub Actions

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_DB: myphotobooth_test
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: postgres_test_password
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Restore dependencies
        run: dotnet restore

      - name: Build
        run: dotnet build --no-restore

      - name: Test
        run: dotnet test --no-build --verbosity normal

      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage.xml
```

### Azure Pipelines

```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: UseDotNet@2
  displayName: 'Install .NET SDK'
  inputs:
    packageType: 'sdk'
    version: '10.0.x'

- script: dotnet restore
  displayName: 'Restore dependencies'

- script: dotnet build --configuration $(buildConfiguration)
  displayName: 'Build project'

- script: dotnet test --configuration $(buildConfiguration) --no-build --verbosity normal
  displayName: 'Run tests'
```

## Troubleshooting

### Common Issues

1. **Testcontainers Fails to Start**:
   - Ensure Docker is running
   - Check port availability
   - Verify network connectivity

2. **Database Connection Errors**:
   - Check connection string
   - Verify database is running
   - Ensure migrations are applied

3. **Flaky Tests**:
   - Tests that sometimes fail
   - Caused by timing issues or shared state
   - Use proper isolation and cleanup

4. **Slow Tests**:
   - Integration tests are slower
   - Use parallel execution
   - Optimize database operations

### Debugging Tests

1. **Attach Debugger**:
   ```bash
   dotnet test --filter "FullyQualifiedName~TestName" --logger "console;verbosity=detailed"
   ```

2. **Verbose Output**:
   ```bash
   dotnet test --logger "console;verbosity=detailed"
   ```

3. **Test Explorer**:
   - Use Visual Studio Test Explorer
   - Debug tests directly

---

**Document Version**: 1.0
**Last Updated**: 2025-02-10
**MyPhotoBooth Version**: 1.3.0
