using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Auth.Commands;
using MyPhotoBooth.Application.Features.Auth.Handlers;
using MyPhotoBooth.Application.Interfaces;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Auth.Handlers;

public class LogoutCommandHandlerTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<LogoutCommandHandler>> _loggerMock;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<LogoutCommandHandler>>();
        _handler = new LogoutCommandHandler(_tokenServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidLogout_ReturnsSuccess()
    {
        // Arrange
        var command = new LogoutCommand("valid-refresh-token");

        _tokenServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(command.RefreshToken, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tokenServiceMock.Verify(
            x => x.RevokeRefreshTokenAsync(command.RefreshToken, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullRefreshToken_StillSucceeds()
    {
        // Arrange
        var command = new LogoutCommand(null!);

        _tokenServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(null, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ServiceThrowsException_PropagatesException()
    {
        // Arrange
        var command = new LogoutCommand("token");
        var expectedException = new InvalidOperationException("Service error");

        _tokenServiceMock
            .Setup(x => x.RevokeRefreshTokenAsync(command.RefreshToken, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Should().Be(expectedException);
    }
}
