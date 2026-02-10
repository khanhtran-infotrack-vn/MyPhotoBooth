using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Auth.Commands;
using MyPhotoBooth.Application.Features.Auth.Handlers;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Auth.Handlers;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<ResetPasswordCommandHandler>> _loggerMock;
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _loggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>();
        _handler = new ResetPasswordCommandHandler(_userManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidToken_ResetsPasswordSuccessfully()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid-reset-token";
        var newPassword = "NewPassword123!";
        var command = new ResetPasswordCommand(email, token, newPassword);

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = email
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(
            x => x.ResetPasswordAsync(user, token, newPassword),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ReturnsFailure()
    {
        // Arrange
        var command = new ResetPasswordCommand("nonexistent@example.com", "token", "NewPassword123!");

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid or expired token");
    }

    [Fact]
    public async Task Handle_InvalidToken_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var token = "invalid-token";
        var newPassword = "NewPassword123!";
        var command = new ResetPasswordCommand(email, token, newPassword);

        var user = new ApplicationUser { Id = "user-id", Email = email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var errors = new[] { new IdentityError { Description = "Invalid token" } };
        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to reset password");
        result.Error.Should().Contain("Invalid token");
    }

    [Fact]
    public async Task Handle_WeakPassword_ReturnsFailure()
    {
        // Arrange
        var email = "test@example.com";
        var token = "valid-token";
        var newPassword = "weak";
        var command = new ResetPasswordCommand(email, token, newPassword);

        var user = new ApplicationUser { Id = "user-id", Email = email };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        var errors = new[] { new IdentityError { Description = "Password too weak" } };
        _userManagerMock
            .Setup(x => x.ResetPasswordAsync(user, token, newPassword))
            .ReturnsAsync(IdentityResult.Failed(errors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Password too weak");
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
