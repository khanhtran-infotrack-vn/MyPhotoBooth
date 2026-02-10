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

public class RemovePhotosFromAlbumCommandHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<ILogger<RemovePhotosFromAlbumCommandHandler>> _loggerMock;
    private readonly RemovePhotosFromAlbumCommandHandler _handler;

    public RemovePhotosFromAlbumCommandHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _loggerMock = new Mock<ILogger<RemovePhotosFromAlbumCommandHandler>>();
        _handler = new RemovePhotosFromAlbumCommandHandler(
            _albumRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_RemovesPhotosSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new RemovePhotosFromAlbumCommand(albumId, photoIds.ToList(), userId);

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
            .Setup(x => x.RemovePhotoFromAlbumAsync(albumId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _albumRepositoryMock.Verify(
            x => x.RemovePhotoFromAlbumAsync(albumId, photoIds[0], It.IsAny<CancellationToken>()),
            Times.Once);
        _albumRepositoryMock.Verify(
            x => x.RemovePhotoFromAlbumAsync(albumId, photoIds[1], It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid() };
        var command = new RemovePhotosFromAlbumCommand(albumId, photoIds.ToList(), userId);

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Album?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Album not found");
        _albumRepositoryMock.Verify(
            x => x.RemovePhotoFromAlbumAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid() };
        var command = new RemovePhotosFromAlbumCommand(albumId, photoIds.ToList(), userId);

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
        _albumRepositoryMock.Verify(
            x => x.RemovePhotoFromAlbumAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_EmptyPhotoList_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new RemovePhotosFromAlbumCommand(albumId, Array.Empty<Guid>().ToList(), userId);

        var album = new Album { Id = albumId, UserId = userId };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _albumRepositoryMock.Verify(
            x => x.RemovePhotoFromAlbumAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SinglePhoto_RemovesSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var command = new RemovePhotosFromAlbumCommand(albumId, new[] { photoId }.ToList(), userId);

        var album = new Album { Id = albumId, UserId = userId };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _albumRepositoryMock
            .Setup(x => x.RemovePhotoFromAlbumAsync(albumId, photoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _albumRepositoryMock.Verify(
            x => x.RemovePhotoFromAlbumAsync(albumId, photoId, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
