using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Features.Albums.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Albums.Handlers;

public class DeleteAlbumCommandHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<ILogger<DeleteAlbumCommandHandler>> _loggerMock;
    private readonly DeleteAlbumCommandHandler _handler;

    public DeleteAlbumCommandHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _loggerMock = new Mock<ILogger<DeleteAlbumCommandHandler>>();
        _handler = new DeleteAlbumCommandHandler(_albumRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_DeletesAlbumSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new DeleteAlbumCommand(albumId, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            Name = "Test Album"
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _albumRepositoryMock
            .Setup(x => x.DeleteAsync(albumId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _albumRepositoryMock.Verify(x => x.DeleteAsync(albumId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new DeleteAlbumCommand(albumId, userId);

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Album?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Album not found");
        _albumRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new DeleteAlbumCommand(albumId, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = "different-user-id"
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
        _albumRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryException_PropagatesException()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new DeleteAlbumCommand(albumId, userId);

        var album = new Album { Id = albumId, UserId = userId };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _albumRepositoryMock
            .Setup(x => x.DeleteAsync(albumId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
