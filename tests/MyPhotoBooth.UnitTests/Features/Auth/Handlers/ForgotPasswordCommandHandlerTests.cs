using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Auth.Commands;
using MyPhotoBooth.Application.Features.Auth.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Auth.Handlers;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _loggerMock;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ForgotPasswordCommandHandler>>();
        _handler = new ForgotPasswordCommandHandler(
            _userManagerMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingEmail_SendsResetEmail()
    {
        // Arrange
        var email = "test@example.com";
        var resetToken = "reset-token-123";
        var resetUrlTemplate = "https://example.com/reset?token={token}&email={email}";
        var command = new ForgotPasswordCommand(email, resetUrlTemplate);

        var user = new ApplicationUser
        {
            Id = "user-id",
            Email = email
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync(resetToken);

        _emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(
                email,
                resetToken,
                It.Is<string>(url => url.Contains(Uri.EscapeDataString(resetToken))),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(email, resetToken, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ReturnsSuccessSilently()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var command = new ForgotPasswordCommand(email, "https://example.com/reset");

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _emailServiceMock.Verify(
            x => x.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ValidEmail_GeneratesCorrectResetUrl()
    {
        // Arrange
        var email = "test@example.com";
        var resetToken = "reset-token-with-special%chars";
        var resetUrlTemplate = "https://example.com/reset?token={token}&email={email}";
        var command = new ForgotPasswordCommand(email, resetUrlTemplate);

        var user = new ApplicationUser { Id = "user-id", Email = email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user)).ReturnsAsync(resetToken);

        string? capturedUrl = null;
        _emailServiceMock
            .Setup(x => x.SendPasswordResetEmailAsync(email, resetToken, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((_, _, url, _) => capturedUrl = url)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedUrl.Should().NotBeNull();
        capturedUrl.Should().Contain(Uri.EscapeDataString(resetToken));
        capturedUrl.Should().Contain(Uri.EscapeDataString(email));
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
