using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return (false, "Email already registered");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        // Assign default "User" role
        await _userManager.AddToRoleAsync(user, "User");

        return (true, null);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return (false, null, "Invalid email or password");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return (false, null, "Invalid email or password");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);

        var expirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");

        var response = new AuthResponse(
            accessToken,
            refreshToken.Token,
            DateTime.UtcNow.AddMinutes(expirationMinutes)
        );

        return (true, response, null);
    }

    public async Task<(bool Success, AuthResponse? Response, string? Error)> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _tokenService.GetRefreshTokenAsync(refreshToken, cancellationToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            return (false, null, "Invalid or expired refresh token");
        }

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user.Id, cancellationToken);

        // Revoke old refresh token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, newRefreshToken.Token, cancellationToken);

        var expirationMinutes = int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15");

        var response = new AuthResponse(
            newAccessToken,
            newRefreshToken.Token,
            DateTime.UtcNow.AddMinutes(expirationMinutes)
        );

        return (true, response, null);
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, null, cancellationToken);
        return true;
    }
}
