using MyPhotoBooth.Application.Common.DTOs;

namespace MyPhotoBooth.Application.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<(bool Success, AuthResponse? Response, string? Error)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
