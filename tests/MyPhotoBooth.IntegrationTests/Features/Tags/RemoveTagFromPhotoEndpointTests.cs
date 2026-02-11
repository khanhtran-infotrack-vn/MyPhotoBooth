using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MyPhotoBooth.IntegrationTests.Fixtures;
using Xunit;

namespace MyPhotoBooth.IntegrationTests.Features.Tags;

[Collection("Database Collection")]
public class RemoveTagFromPhotoEndpointTests
{
    private readonly PostgreSqlFixture _dbFixture;

    public RemoveTagFromPhotoEndpointTests(PostgreSqlFixture dbFixture)
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
    public async Task RemoveTagFromPhoto_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"remove-tag-photo-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create a tag
        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "nature" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        // Upload a photo
        using var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }); // PNG header
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        formData.Add(fileContent, "file", "test.png");
        var uploadResponse = await client.PostAsync("/api/photos", formData);
        var uploadContent = await uploadResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var photoId = uploadContent.GetProperty("id").GetString();

        // Add tag to photo
        await client.PostAsJsonAsync($"/api/photos/{photoId}/tags", new { tagIds = new[] { tagId } });

        // Act
        var response = await client.DeleteAsync($"/api/tags/{tagId}/photos/{photoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveTagFromPhoto_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        // Act
        var response = await client.DeleteAsync($"/api/tags/{Guid.NewGuid()}/photos/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RemoveTagFromPhoto_DifferentUser_ReturnsError()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        // User 1: Create tag and photo
        var email1 = $"remove-tag-user1-{Guid.NewGuid()}@example.com";
        var token1 = await RegisterAndLoginAsync(client, email1, "TestPassword123", "User 1");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);

        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "nature" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        using var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        formData.Add(fileContent, "file", "test.png");
        var uploadResponse = await client.PostAsync("/api/photos", formData);
        var uploadContent = await uploadResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var photoId = uploadContent.GetProperty("id").GetString();

        // User 2: Try to remove tag from user 1's photo
        var email2 = $"remove-tag-user2-{Guid.NewGuid()}@example.com";
        var token2 = await RegisterAndLoginAsync(client, email2, "TestPassword123", "User 2");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);

        // Act
        var response = await client.DeleteAsync($"/api/tags/{tagId}/photos/{photoId}");

        // Assert - The request should fail because the photo belongs to user 1
        response.StatusCode.Should().NotBe(HttpStatusCode.NoContent);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveTagFromPhoto_TagNotAttached_ReturnsNoContent()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"remove-tag-not-attached-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "nature" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        using var formData = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        formData.Add(fileContent, "file", "test.png");
        var uploadResponse = await client.PostAsync("/api/photos", formData);
        var uploadContent = await uploadResponse.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var photoId = uploadContent.GetProperty("id").GetString();

        // Act - Remove tag that was never added
        var response = await client.DeleteAsync($"/api/tags/{tagId}/photos/{photoId}");

        // Assert - Should still succeed (idempotent)
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
