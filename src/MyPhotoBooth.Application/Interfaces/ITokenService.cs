using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken> CreateRefreshTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string token, string? replacedByToken = null, CancellationToken cancellationToken = default);
}
