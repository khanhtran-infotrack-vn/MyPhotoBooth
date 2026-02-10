using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Handlers;

public class ToggleFavoritePhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<ToggleFavoritePhotoCommandHandler>> _loggerMock;
    private readonly ToggleFavoritePhotoCommandHandler _handler;

    public ToggleFavoritePhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<ToggleFavoritePhotoCommandHandler>>();
        _handler = new ToggleFavoritePhotoCommandHandler(
            _photoRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_AddsFavorite_WhenNotAlreadyFavorited()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new ToggleFavoritePhotoCommand(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg",
            FilePath = "/path/to/file",
            ThumbnailPath = "/path/to/thumb",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.IsFavoriteAsync(photoId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _photoRepositoryMock
            .Setup(x => x.ToggleFavoriteAsync(photoId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        _photoRepositoryMock.Verify(x => x.ToggleFavoriteAsync(photoId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RemovesFavorite_WhenAlreadyFavorited()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new ToggleFavoritePhotoCommand(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg",
            FilePath = "/path/to/file",
            ThumbnailPath = "/path/to/thumb",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.IsFavoriteAsync(photoId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _photoRepositoryMock
            .Setup(x => x.ToggleFavoriteAsync(photoId, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
        _photoRepositoryMock.Verify(x => x.ToggleFavoriteAsync(photoId, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenPhotoNotFound()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new ToggleFavoritePhotoCommand(photoId, userId);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Photo?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Photo not found");
        _photoRepositoryMock.Verify(x => x.ToggleFavoriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ReturnsFailure_WhenPhotoBelongsToDifferentUser()
    {
        // Arrange
        var photoOwner = "photo-owner";
        var requester = "requester";
        var photoId = Guid.NewGuid();
        var command = new ToggleFavoritePhotoCommand(photoId, requester);

        var photo = new Photo
        {
            Id = photoId,
            UserId = photoOwner,
            OriginalFileName = "test.jpg",
            FilePath = "/path/to/file",
            ThumbnailPath = "/path/to/thumb",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Photo not found");
        _photoRepositoryMock.Verify(x => x.ToggleFavoriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
