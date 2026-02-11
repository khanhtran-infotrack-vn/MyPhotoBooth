using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MyPhotoBooth.IntegrationTests.Fixtures;
using Xunit;

namespace MyPhotoBooth.IntegrationTests.Features.Tags;

[Collection("Database Collection")]
public class GetTagPhotosEndpointTests
{
    private readonly PostgreSqlFixture _dbFixture;

    public GetTagPhotosEndpointTests(PostgreSqlFixture dbFixture)
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
        var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonContent);
        var token = jsonDoc.RootElement.GetProperty("accessToken").GetString()
            ?? throw new Exception($"No access token in response: {jsonContent}");

        return token;
    }

    [Fact]
    public async Task GetTagPhotos_ValidRequest_ReturnsPaginatedPhotos()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"tag-photos-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a tag
        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "nature" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        // Upload multiple photos and add them to the tag
        var photoIds = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            using var formData = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "file", $"test{i}.png");
            var uploadResponse = await client.PostAsync("/api/photos", formData);
            var uploadContent = await uploadResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            var photoId = uploadContent.GetProperty("id").GetString();
            photoIds.Add(photoId);

            await client.PostAsJsonAsync($"/api/photos/{photoId}/tags", new { tagIds = new[] { tagId } });
        }

        // Act
        var response = await client.GetAsync($"/api/tags/{tagId}/photos?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        content.GetProperty("items").ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array);
        content.GetProperty("items").GetArrayLength().Should().Be(5);
        content.GetProperty("totalCount").GetInt32().Should().Be(5);
        content.GetProperty("page").GetInt32().Should().Be(1);
        content.GetProperty("pageSize").GetInt32().Should().Be(10);
        content.GetProperty("totalPages").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetTagPhotos_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/tags/{Guid.NewGuid()}/photos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTagPhotos_InvalidTagId_ReturnsNotFound()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"tag-photos-notfound-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync($"/api/tags/{Guid.NewGuid()}/photos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTagPhotos_DifferentUser_ReturnsNotFound()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        // User 1: Create tag
        var email1 = $"tag-photos-user1-{Guid.NewGuid()}@example.com";
        var token1 = await RegisterAndLoginAsync(client, email1, "TestPassword123", "User 1");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "private-tag" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        // User 2: Try to access user 1's tag
        var email2 = $"tag-photos-user2-{Guid.NewGuid()}@example.com";
        var token2 = await RegisterAndLoginAsync(client, email2, "TestPassword123", "User 2");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await client.GetAsync($"/api/tags/{tagId}/photos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTagPhotos_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"tag-photos-page-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a tag
        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "pagination-test" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        // Upload 15 photos
        for (int i = 0; i < 15; i++)
        {
            using var formData = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "file", $"test{i}.png");
            var uploadResponse = await client.PostAsync("/api/photos", formData);
            var uploadContent = await uploadResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            var photoId = uploadContent.GetProperty("id").GetString();

            await client.PostAsJsonAsync($"/api/photos/{photoId}/tags", new { tagIds = new[] { tagId } });
        }

        // Act - Get page 2 with pageSize 10
        var response = await client.GetAsync($"/api/tags/{tagId}/photos?page=2&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(5);
        content.GetProperty("totalCount").GetInt32().Should().Be(15);
        content.GetProperty("page").GetInt32().Should().Be(2);
        content.GetProperty("pageSize").GetInt32().Should().Be(10);
        content.GetProperty("totalPages").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task GetTagPhotos_EmptyTag_ReturnsEmptyPaginatedResult()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"tag-photos-empty-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "empty-tag" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        // Act
        var response = await client.GetAsync($"/api/tags/{tagId}/photos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(0);
        content.GetProperty("totalCount").GetInt32().Should().Be(0);
    }
}
