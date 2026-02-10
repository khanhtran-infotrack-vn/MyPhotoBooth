using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MyPhotoBooth.IntegrationTests.Fixtures;
using Xunit;

namespace MyPhotoBooth.IntegrationTests.Features.Albums;

[Collection("Database Collection")]
public class AlbumsEndpointTests
{
    private readonly PostgreSqlFixture _dbFixture;

    public AlbumsEndpointTests(PostgreSqlFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    private static async Task<string> RegisterAndLoginAsync(HttpClient client, string email, string password, string displayName)
    {
        var registerRequest = new
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            DisplayName = displayName
        };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var jsonContent = await loginResponse.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonContent);
        var token = jsonDoc.RootElement.GetProperty("accessToken").GetString()
            ?? throw new Exception($"No access token in response: {jsonContent}");

        return token;
    }

    [Fact]
    public async Task CreateAlbum_ValidRequest_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"album-create-ok-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new { Name = "Test Album", Description = "Test Description" };
        var response = await client.PostAsJsonAsync("/api/albums", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAlbum_Unauthorized_ReturnsUnauthorized()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        var request = new { Name = "Test Album", Description = "Test Description" };
        var response = await client.PostAsJsonAsync("/api/albums", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAlbum_EmptyName_ReturnsBadRequest()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"album-empty-name-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new { Name = "", Description = "Test Description" };
        var response = await client.PostAsJsonAsync("/api/albums", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAlbums_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"album-get-ok-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/albums");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.ValueKind.Should().Be(JsonValueKind.Array);
        content.GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetAlbums_Unauthorized_ReturnsUnauthorized()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/albums");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAlbum_ValidRequest_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"album-update-ok-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new { Name = "Original Name", Description = "Original Description" };
        var createResponse = await client.PostAsJsonAsync("/api/albums", createRequest);
        var createdAlbum = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var albumId = createdAlbum.GetProperty("id").GetString();

        var updateRequest = new { Name = "Updated Name", Description = "Updated Description" };
        var response = await client.PutAsJsonAsync($"/api/albums/{albumId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAlbum_ValidRequest_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"album-delete-ok-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new { Name = "To Delete", Description = "Will be deleted" };
        var createResponse = await client.PostAsJsonAsync("/api/albums", createRequest);
        var createdAlbum = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var albumId = createdAlbum.GetProperty("id").GetString();

        var response = await client.DeleteAsync($"/api/albums/{albumId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAlbum_Unauthorized_ReturnsUnauthorized()
    {
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"album-delete-unauth-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createRequest = new { Name = "Test", Description = "Test" };
        var createResponse = await client.PostAsJsonAsync("/api/albums", createRequest);
        var createdAlbum = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var albumId = createdAlbum.GetProperty("id").GetString();

        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.DeleteAsync($"/api/albums/{albumId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
