using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MyPhotoBooth.IntegrationTests.Fixtures;
using Xunit;

namespace MyPhotoBooth.IntegrationTests.Features.Auth;

[Collection("Database Collection")]
public class LoginEndpointTests
{
    private readonly PostgreSqlFixture _dbFixture;

    public LoginEndpointTests(PostgreSqlFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    private static async Task RegisterUserAsync(HttpClient client, string email, string password)
    {
        var registerRequest = new
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            DisplayName = "Test User"
        };
        var response = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithTokens()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"login-test-{Guid.NewGuid()}@example.com";
        var password = "TestPassword123";
        await RegisterUserAsync(client, email, password);

        var loginRequest = new { Email = email, Password = password };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonContent);
        jsonDoc.RootElement.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        // RefreshToken is now in httpOnly cookie, not in response body
        response.Headers.Contains("Set-Cookie").Should().BeTrue();
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var loginRequest = new { Email = "nonexistent@example.com", Password = "TestPassword123" };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"login-test-{Guid.NewGuid()}@example.com";
        await RegisterUserAsync(client, email, "TestPassword123");

        var loginRequest = new { Email = email, Password = "WrongPassword123" };
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_MissingEmail_ReturnsBadRequest()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var loginRequest = new { Email = "", Password = "TestPassword123" };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_MissingPassword_ReturnsBadRequest()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var loginRequest = new { Email = "test@example.com", Password = "" };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
