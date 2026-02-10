using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace MyPhotoBooth.IntegrationTests.Helpers;

public static class TestAuthHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string email, string password, string displayName)
    {
        // Register
        var registerRequest = new
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            DisplayName = displayName
        };
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        // Login
        var loginRequest = new { Email = email, Password = password };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var jsonContent = await loginResponse.Content.ReadAsStringAsync();
        var authResponse = JsonSerializer.Deserialize<AuthResponse>(jsonContent, _jsonOptions);

        if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
        {
            throw new Exception($"Failed to get access token. Response: {jsonContent}");
        }

        // Verify the token is not just whitespace
        var token = authResponse.AccessToken.Trim();
        if (string.IsNullOrEmpty(token) || token == "null" || token.Length < 50)
        {
            throw new Exception($"Invalid access token received. Token length: {token?.Length ?? 0}. Response: {jsonContent}");
        }

        return token;
    }

    public static void SetAuthHeader(HttpClient client, string token)
    {
        // Clear any existing auth header first
        client.DefaultRequestHeaders.Remove("Authorization");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearAuthHeader(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("Authorization");
    }

    private record AuthResponse([property: JsonPropertyName("accessToken")] string AccessToken, [property: JsonPropertyName("refreshToken")] string RefreshToken, [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt);
}
