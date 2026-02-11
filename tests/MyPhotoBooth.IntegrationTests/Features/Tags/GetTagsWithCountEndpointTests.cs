using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using MyPhotoBooth.IntegrationTests.Fixtures;
using Xunit;

namespace MyPhotoBooth.IntegrationTests.Features.Tags;

[Collection("Database Collection")]
public class GetTagsWithCountEndpointTests
{
    private readonly PostgreSqlFixture _dbFixture;

    public GetTagsWithCountEndpointTests(PostgreSqlFixture dbFixture)
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
    public async Task GetTagsWithCount_ReturnsTagsWithPhotoCounts()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"tags-count-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Create tags
        var tag1Response = await client.PostAsJsonAsync("/api/tags", new { Name = "nature" });
        var tag2Response = await client.PostAsJsonAsync("/api/tags", new { Name = "travel" });
        var tag1Content = await tag1Response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tag2Content = await tag2Response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var tag1Id = tag1Content.GetProperty("id").GetString();
        var tag2Id = tag2Content.GetProperty("id").GetString();

        // Upload photos and add tags
        using var formData1 = new MultipartFormDataContent();
        var fileContent1 = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        fileContent1.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        formData1.Add(fileContent1, "file", "test1.png");
        var upload1Response = await client.PostAsync("/api/photos", formData1);
        var upload1Content = await upload1Response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var photo1Id = upload1Content.GetProperty("id").GetString();

        using var formData2 = new MultipartFormDataContent();
        var fileContent2 = new ByteArrayContent(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        fileContent2.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        formData2.Add(fileContent2, "file", "test2.png");
        var upload2Response = await client.PostAsync("/api/photos", formData2);
        var upload2Content = await upload2Response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var photo2Id = upload2Content.GetProperty("id").GetString();

        // Add tag1 to both photos, tag2 to one photo
        await client.PostAsJsonAsync($"/api/photos/{photo1Id}/tags", new { tagIds = new[] { tag1Id } });
        await client.PostAsJsonAsync($"/api/photos/{photo2Id}/tags", new { tagIds = new[] { tag1Id, tag2Id } });

        // Act
        var response = await client.GetAsync("/api/tags/with-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        content.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array);

        var tags = content.EnumerateArray().ToList();
        var natureTag = tags.FirstOrDefault(t => t.GetProperty("name").GetString() == "nature");
        var travelTag = tags.FirstOrDefault(t => t.GetProperty("name").GetString() == "travel");

        natureTag.Should().NotBeNull();
        natureTag.GetProperty("photoCount").GetInt32().Should().Be(2);

        travelTag.Should().NotBeNull();
        travelTag.GetProperty("photoCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetTagsWithCount_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/tags/with-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTagsWithCount_EmptyList_ReturnsEmptyArray()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"tags-count-empty-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/tags/with-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        content.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array);
        content.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetTagsWithCount_TagWithNoPhotos_ReturnsZeroCount()
    {
        // Arrange
        using var factory = new TestWebApplicationFactory(_dbFixture.ConnectionString);
        using var client = factory.CreateClient();
        var email = $"tags-count-zero-{Guid.NewGuid()}@example.com";
        var token = await RegisterAndLoginAsync(client, email, "TestPassword123", "Test User");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsJsonAsync("/api/tags", new { Name = "unused-tag" });

        // Act
        var response = await client.GetAsync("/api/tags/with-count");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var unusedTag = content.EnumerateArray().FirstOrDefault();
        unusedTag.GetProperty("photoCount").GetInt32().Should().Be(0);
    }
}
