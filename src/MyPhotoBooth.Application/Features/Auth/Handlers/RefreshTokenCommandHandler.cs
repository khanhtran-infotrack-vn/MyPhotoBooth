using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Auth.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Auth.Handlers;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var storedToken = await _tokenService.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            _logger.LogWarning("Refresh token attempt failed: invalid or expired token");
            return Result.Failure<AuthResponse>(Errors.Auth.InvalidToken);
        }

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);

        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, newRefreshToken.Token, cancellationToken);

        var expirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");

        var response = new AuthResponse(
            newAccessToken,
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(expirationMinutes)
        );

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);
        return Result.Success(response);
    }
}
