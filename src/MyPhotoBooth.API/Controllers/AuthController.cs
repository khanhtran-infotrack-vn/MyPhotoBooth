using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.API.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Auth.Commands;

namespace MyPhotoBooth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;

    public AuthController(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.DisplayName);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(new { message = "Registration successful" });
        return BadRequest(new { message = result.Error });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            var response = result.Value;

            // Set refresh token in httpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/",
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

            // Return access token in response body (short-lived, acceptable)
            return Ok(new LoginResponse(response.AccessToken, response.ExpiresAt));
        }
        return Unauthorized(new { message = result.Error });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        // Read refresh token from httpOnly cookie
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Refresh token not found" });
        }

        var command = new RefreshTokenCommand(refreshToken);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            var response = result.Value;

            // Update refresh token in cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/",
                IsEssential = true
            };

            Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

            // Return access token in response body
            return Ok(new LoginResponse(response.AccessToken, response.ExpiresAt));
        }
        return Unauthorized(new { message = result.Error });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        // Read refresh token from httpOnly cookie
        var refreshToken = Request.Cookies["refreshToken"];

        var command = new LogoutCommand(refreshToken ?? string.Empty);
        var result = await _sender.Send(command, cancellationToken);

        // Clear the cookie regardless of result
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/",
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict
        });

        if (result.IsSuccess)
            return Ok(new { message = "Logout successful" });
        return BadRequest(new { message = result.Error });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var scheme = HttpContext.Request.Scheme;
        var host = HttpContext.Request.Host.Value;
        var resetUrlTemplate = $"{scheme}://{host}/reset-password?token={{token}}&email={{email}}";

        var command = new ForgotPasswordCommand(request.Email, resetUrlTemplate);
        await _sender.Send(command, cancellationToken);

        // Always return success to avoid revealing user existence
        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
            return Ok(new { message = "Password has been reset successfully. You can now login with your new password." });
        return BadRequest(new { message = result.Error });
    }
}

// Response DTO for login without refresh token (now in cookie)
public record LoginResponse(
    string AccessToken,
    DateTime ExpiresAt
);
