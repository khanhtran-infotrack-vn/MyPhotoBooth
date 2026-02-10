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

public class DeletePhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<DeletePhotoCommandHandler>> _loggerMock;
    private readonly DeletePhotoCommandHandler _handler;

    public DeletePhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<DeletePhotoCommandHandler>>();
        _handler = new DeletePhotoCommandHandler(
            _photoRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_DeletesPhotoSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new DeletePhotoCommand(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            FilePath = "/storage/original.jpg",
            ThumbnailPath = "/storage/thumb.jpg"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _fileStorageServiceMock
            .Setup(x => x.DeleteFileAsync(photo.FilePath, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _fileStorageServiceMock
            .Setup(x => x.DeleteFileAsync(photo.ThumbnailPath, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _photoRepositoryMock
            .Setup(x => x.DeleteAsync(photoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(photo.FilePath, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(photo.ThumbnailPath, It.IsAny<CancellationToken>()), Times.Once);
        _photoRepositoryMock.Verify(x => x.DeleteAsync(photoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new DeletePhotoCommand(photoId, userId);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Photo?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Photo not found");
        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new DeletePhotoCommand(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = "different-user-id"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StorageErrorDuringDeletion_PropagatesError()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new DeletePhotoCommand(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            FilePath = "/storage/original.jpg",
            ThumbnailPath = "/storage/thumb.jpg"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _fileStorageServiceMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Storage error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
