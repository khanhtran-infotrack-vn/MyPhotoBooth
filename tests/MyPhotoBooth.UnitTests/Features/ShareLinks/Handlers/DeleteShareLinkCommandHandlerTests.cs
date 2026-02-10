using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Application.Features.ShareLinks.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.ShareLinks.Handlers;

public class DeleteShareLinkCommandHandlerTests
{
    private readonly Mock<IShareLinkRepository> _shareLinkRepositoryMock;
    private readonly Mock<ILogger<DeleteShareLinkCommandHandler>> _loggerMock;
    private readonly DeleteShareLinkCommandHandler _handler;

    public DeleteShareLinkCommandHandlerTests()
    {
        _shareLinkRepositoryMock = new Mock<IShareLinkRepository>();
        _loggerMock = new Mock<ILogger<DeleteShareLinkCommandHandler>>();
        _handler = new DeleteShareLinkCommandHandler(
            _shareLinkRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_RevokesShareLink()
    {
        // Arrange
        var userId = "user-id";
        var shareLinkId = Guid.NewGuid();
        var command = new DeleteShareLinkCommand(shareLinkId, userId);

        var shareLink = new ShareLink
        {
            Id = shareLinkId,
            UserId = userId,
            Token = "abc123",
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByIdAsync(shareLinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        _shareLinkRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        shareLink.RevokedAt.Should().NotBeNull();
        shareLink.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _shareLinkRepositoryMock.Verify(
            x => x.UpdateAsync(shareLink, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentShareLink_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var shareLinkId = Guid.NewGuid();
        var command = new DeleteShareLinkCommand(shareLinkId, userId);

        _shareLinkRepositoryMock
            .Setup(x => x.GetByIdAsync(shareLinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShareLink?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Share link not found");
        _shareLinkRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var shareLinkId = Guid.NewGuid();
        var command = new DeleteShareLinkCommand(shareLinkId, userId);

        var shareLink = new ShareLink
        {
            Id = shareLinkId,
            UserId = "different-user-id"
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByIdAsync(shareLinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
        _shareLinkRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyRevokedLink_UpdatesRevokedAt()
    {
        // Arrange
        var userId = "user-id";
        var shareLinkId = Guid.NewGuid();
        var previousRevokedAt = DateTime.UtcNow.AddDays(-1);
        var command = new DeleteShareLinkCommand(shareLinkId, userId);

        var shareLink = new ShareLink
        {
            Id = shareLinkId,
            UserId = userId,
            RevokedAt = previousRevokedAt
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByIdAsync(shareLinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        _shareLinkRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        shareLink.RevokedAt.Should().BeAfter(previousRevokedAt);
    }

    [Fact]
    public async Task Handle_RepositoryException_PropagatesException()
    {
        // Arrange
        var userId = "user-id";
        var shareLinkId = Guid.NewGuid();
        var command = new DeleteShareLinkCommand(shareLinkId, userId);

        var shareLink = new ShareLink { Id = shareLinkId, UserId = userId };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByIdAsync(shareLinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        _shareLinkRepositoryMock
            .Setup(x => x.UpdateAsync(shareLink, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithExpiredLink_StillRevokesSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var shareLinkId = Guid.NewGuid();
        var command = new DeleteShareLinkCommand(shareLinkId, userId);

        var shareLink = new ShareLink
        {
            Id = shareLinkId,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByIdAsync(shareLinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        _shareLinkRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        shareLink.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_DoesNotDeleteShareLink_SetsRevokedAt()
    {
        // Arrange
        var userId = "user-id";
        var shareLinkId = Guid.NewGuid();
        var command = new DeleteShareLinkCommand(shareLinkId, userId);

        var shareLink = new ShareLink
        {
            Id = shareLinkId,
            UserId = userId
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByIdAsync(shareLinkId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        _shareLinkRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _shareLinkRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _shareLinkRepositoryMock.Verify(
            x => x.UpdateAsync(shareLink, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
