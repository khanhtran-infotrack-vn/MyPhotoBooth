using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MyPhotoBooth.IntegrationTests.Fixtures;
using Xunit;

namespace MyPhotoBooth.IntegrationTests.Features.Photos;

[Collection("Database Collection")]
public class SearchPhotosEndpointsTests
{
    private readonly PostgreSqlFixture _dbFixture;

    public SearchPhotosEndpointsTests(PostgreSqlFixture dbFixture)
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

    private static async Task<Guid> UploadTestPhotoAsync(HttpClient client, string fileName = "test.jpg", string? description = null)
    {
        // Create a minimal valid PNG image (1x1 red pixel)
        var pngBytes = GetMinimalPng();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(pngBytes);
        // Use PNG content type since we're generating a PNG
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        var actualFileName = fileName.EndsWith(".jpg") ? fileName.Replace(".jpg", ".png") : fileName;
        content.Add(fileContent, "file", actualFileName);

        if (description != null)
        {
            content.Add(new StringContent(description), "description");
        }

        var response = await client.PostAsync("/api/photos", content);
        response.EnsureSuccessStatusCode();

        var jsonContent = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(jsonContent);
        return Guid.Parse(jsonDoc.RootElement.GetProperty("id").GetString()!);
    }

    // Minimal valid PNG (1x1 red pixel)
    private static byte[] GetMinimalPng()
    {
        return new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, // IHDR chunk length (13)
            0x49, 0x48, 0x44, 0x52, // IHDR chunk type
            0x00, 0x00, 0x00, 0x01, // Width: 1
            0x00, 0x00, 0x00, 0x01, // Height: 1
            0x08, 0x02, 0x00, 0x00, 0x00, // Bit depth: 8, Color type: 2 (RGB), others: 0
            0x4F, 0x70, 0x06, 0x93, // IHDR CRC
            0x00, 0x00, 0x00, 0x0C, // IDAT chunk length (12)
            0x49, 0x44, 0x41, 0x54, // IDAT chunk type
            0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00, 0x03, 0x01, 0x01, 0x00, // Image data (compressed 1x1 red pixel)
            0x18, 0xDD, 0x8D, 0xB4, // IDAT CRC
            0x00, 0x00, 0x00, 0x00, // IEND chunk length (0)
            0x49, 0x45, 0x4E, 0x44, // IEND chunk type
            0xAE, 0x42, 0x60, 0x82  // IEND CRC
        };
    }

    [Fact]
    public async Task SearchPhotos_ByFilename_ReturnsMatchingPhotos()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"search-filename-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await UploadTestPhotoAsync(client, "vacation-beach.jpg");
        await UploadTestPhotoAsync(client, "vacation-mountains.jpg");
        await UploadTestPhotoAsync(client, "family-photo.jpg");

        // Act
        var response = await client.GetAsync("/api/photos/search?q=vacation");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(2);
        content.GetProperty("totalCount").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task SearchPhotos_ByDescription_ReturnsMatchingPhotos()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"search-desc-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await UploadTestPhotoAsync(client, "photo1.jpg", "Beautiful sunset at the beach");
        await UploadTestPhotoAsync(client, "photo2.jpg", "Sunrise over the mountains");

        // Act
        var response = await client.GetAsync("/api/photos/search?q=sunset");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(1);
        content.GetProperty("totalCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task SearchPhotos_ByTag_ReturnsMatchingPhotos()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"search-tag-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create tag
        var tagResponse = await client.PostAsJsonAsync("/api/tags", new { Name = "nature" });
        var tagContent = await tagResponse.Content.ReadFromJsonAsync<JsonElement>();
        var tagId = tagContent.GetProperty("id").GetString();

        // Upload photo
        var photoId = await UploadTestPhotoAsync(client, "photo.jpg");

        // Add tag to photo
        await client.PostAsJsonAsync($"/api/photos/{photoId}/tags", new[] { tagId });

        // Act
        var response = await client.GetAsync("/api/photos/search?q=nature");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task SearchPhotos_ByAlbum_ReturnsMatchingPhotos()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"search-album-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create album
        var albumResponse = await client.PostAsJsonAsync("/api/albums", new { Name = "Summer Vacation", Description = "Summer 2024" });
        var albumContent = await albumResponse.Content.ReadFromJsonAsync<JsonElement>();
        var albumId = albumContent.GetProperty("id").GetString();

        // Upload photo
        var photoId = await UploadTestPhotoAsync(client, "photo.jpg");

        // Add photo to album
        await client.PostAsJsonAsync($"/api/albums/{albumId}/photos", new[] { photoId.ToString() });

        // Act
        var response = await client.GetAsync("/api/photos/search?q=summer");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task SearchPhotos_NoResults_ReturnsEmptyList()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"search-empty-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await UploadTestPhotoAsync(client, "photo.jpg");

        // Act
        var response = await client.GetAsync("/api/photos/search?q=nonexistentterm");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(0);
        content.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task SearchPhotos_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/photos/search?q=test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SearchPhotos_SearchTermTooShort_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"search-short-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/photos/search?q=a");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchPhotos_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"search-page-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Upload many matching photos
        for (int i = 0; i < 15; i++)
        {
            await UploadTestPhotoAsync(client, $"test-photo-{i}.jpg");
        }

        // Act - get second page
        var response = await client.GetAsync("/api/photos/search?q=test&page=2&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(5);
        content.GetProperty("page").GetInt32().Should().Be(2);
        content.GetProperty("totalCount").GetInt32().Should().Be(15);
        content.GetProperty("totalPages").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task SearchPhotos_OnlyReturnsUserPhotos_WhenMultipleUsersExist()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        // First user
        var email1 = $"search-user1-{Guid.NewGuid()}@example.com";
        var token1 = await RegisterAndLoginAsync(client, email1, "TestPassword123", "User 1");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        await UploadTestPhotoAsync(client, "vacation1.jpg");

        // Second user
        var email2 = $"search-user2-{Guid.NewGuid()}@example.com";
        var token2 = await RegisterAndLoginAsync(client, email2, "TestPassword123", "User 2");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token2);
        await UploadTestPhotoAsync(client, "vacation2.jpg");

        // Act - search as first user
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token1);
        var response = await client.GetAsync("/api/photos/search?q=vacation");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        content.GetProperty("items").GetArrayLength().Should().Be(1);
        content.GetProperty("totalCount").GetInt32().Should().Be(1);
    }
}
