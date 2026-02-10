using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Auth.Commands;
using MyPhotoBooth.Application.Features.Auth.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Auth.Handlers;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _tokenServiceMock = new Mock<ITokenService>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<RefreshTokenCommandHandler>>();

        _configurationMock.Setup(c => c["JwtSettings:AccessTokenExpirationMinutes"]).Returns("15");

        _handler = new RefreshTokenCommandHandler(
            _userManagerMock.Object,
            _tokenServiceMock.Object,
            _configurationMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRefreshToken_ReturnsNewAuthResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand("valid-refresh-token");
        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = "test@example.com",
            UserName = "test@example.com"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = command.RefreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = null
        };

        _tokenServiceMock
            .Setup(x => x.GetRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new[] { "User" });

        _tokenServiceMock
            .Setup(x => x.GenerateAccessToken(user, It.IsAny<IList<string>>()))
            .Returns("new-access-token");

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "new-refresh-token",
            UserId = user.Id
        };

        _tokenServiceMock
            .Setup(x => x.CreateRefreshTokenAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RefreshToken.Should().Be("new-refresh-token");

        _tokenServiceMock.Verify(
            x => x.RevokeRefreshTokenAsync(command.RefreshToken, "new-refresh-token", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentToken_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("non-existent-token");

        _tokenServiceMock
            .Setup(x => x.GetRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid or expired token");
    }

    [Fact]
    public async Task Handle_InactiveToken_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("inactive-token");
        var user = new ApplicationUser { Id = "user-id" };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = command.RefreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            RevokedAt = DateTime.UtcNow
        };

        _tokenServiceMock
            .Setup(x => x.GetRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid or expired token");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshTokenCommand("expired-token");
        var user = new ApplicationUser { Id = "user-id" };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = command.RefreshToken,
            UserId = user.Id,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            RevokedAt = null
        };

        _tokenServiceMock
            .Setup(x => x.GetRefreshTokenAsync(command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid or expired token");
    }

    private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }
}
