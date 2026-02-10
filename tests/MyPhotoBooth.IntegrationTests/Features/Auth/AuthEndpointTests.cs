using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using MyPhotoBooth.IntegrationTests.Fixtures;

namespace MyPhotoBooth.IntegrationTests.Features.Auth;

[Collection("Database Collection")]
public class AuthEndpointTests : IAsyncLifetime
{
    private readonly PostgreSqlFixture _dbFixture;
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthEndpointTests(PostgreSqlFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = new TestWebApplicationFactory(dbFixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _factory.Dispose();
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsOk()
    {
        // Arrange
        var uniqueEmail = $"test-{Guid.NewGuid()}@example.com";
        var request = new
        {
            Email = uniqueEmail,
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123",
            DisplayName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var uniqueEmail = $"test-{Guid.NewGuid()}@example.com";
        var request = new
        {
            Email = uniqueEmail,
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123",
            DisplayName = "Test User"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "not-an-email",
            Password = "TestPassword123",
            ConfirmPassword = "TestPassword123",
            DisplayName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var uniqueEmail = $"test-{Guid.NewGuid()}@example.com";
        var request = new
        {
            Email = uniqueEmail,
            Password = "weak",
            ConfirmPassword = "weak",
            DisplayName = "Test User"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
