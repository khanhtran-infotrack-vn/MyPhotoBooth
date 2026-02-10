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

public class AddPhotosToAlbumCommandHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<AddPhotosToAlbumCommandHandler>> _loggerMock;
    private readonly AddPhotosToAlbumCommandHandler _handler;

    public AddPhotosToAlbumCommandHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<AddPhotosToAlbumCommandHandler>>();
        _handler = new AddPhotosToAlbumCommandHandler(
            _albumRepositoryMock.Object,
            _photoRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_AddsPhotosSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new AddPhotosToAlbumCommand(albumId, photoIds.ToList(), userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            Name = "Test Album",
            AlbumPhotos = new List<AlbumPhoto>()
        };

        var photo1 = new Photo { Id = photoIds[0], UserId = userId };
        var photo2 = new Photo { Id = photoIds[1], UserId = userId };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoIds[0], It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo1);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoIds[1], It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo2);

        _albumRepositoryMock
            .Setup(x => x.AddPhotoToAlbumAsync(albumId, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _albumRepositoryMock.Verify(
            x => x.AddPhotoToAlbumAsync(albumId, photoIds[0], 0, It.IsAny<CancellationToken>()),
            Times.Once);
        _albumRepositoryMock.Verify(
            x => x.AddPhotoToAlbumAsync(albumId, photoIds[1], 0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid() };
        var command = new AddPhotosToAlbumCommand(albumId, photoIds.ToList(), userId);

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Album?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Album not found");
    }

    [Fact]
    public async Task Handle_UnauthorizedAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid() };
        var command = new AddPhotosToAlbumCommand(albumId, photoIds.ToList(), userId);

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
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid() };
        var command = new AddPhotosToAlbumCommand(albumId, photoIds.ToList(), userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            AlbumPhotos = new List<AlbumPhoto>()
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoIds[0], It.IsAny<CancellationToken>()))
            .ReturnsAsync((Photo?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Photo not found");
    }

    [Fact]
    public async Task Handle_UnauthorizedPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoIds = new[] { Guid.NewGuid() };
        var command = new AddPhotosToAlbumCommand(albumId, photoIds.ToList(), userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            AlbumPhotos = new List<AlbumPhoto>()
        };

        var photo = new Photo
        {
            Id = photoIds[0],
            UserId = "different-user-id"
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoIds[0], It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
    }

    [Fact]
    public async Task Handle_AlreadyExistingPhoto_SkipsAdding()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var photoId = Guid.NewGuid();
        var command = new AddPhotosToAlbumCommand(albumId, new[] { photoId }.ToList(), userId);

        var albumPhoto = new AlbumPhoto
        {
            AlbumId = albumId,
            PhotoId = photoId,
            SortOrder = 0,
            Photo = new Photo { Id = photoId, UserId = userId }
        };

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            AlbumPhotos = new List<AlbumPhoto> { albumPhoto }
        };

        var photo = new Photo { Id = photoId, UserId = userId };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _albumRepositoryMock.Verify(
            x => x.AddPhotoToAlbumAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingPhotos_CalculatesCorrectSortOrder()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var newPhotoId = Guid.NewGuid();
        var command = new AddPhotosToAlbumCommand(albumId, new[] { newPhotoId }.ToList(), userId);

        var existingPhoto = new Photo { Id = Guid.NewGuid(), UserId = userId };
        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            AlbumPhotos = new List<AlbumPhoto>
            {
                new AlbumPhoto { PhotoId = existingPhoto.Id, SortOrder = 0, Photo = existingPhoto },
                new AlbumPhoto { PhotoId = Guid.NewGuid(), SortOrder = 1, Photo = new Photo { UserId = userId } }
            }
        };

        var newPhoto = new Photo { Id = newPhotoId, UserId = userId };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(newPhotoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPhoto);

        _albumRepositoryMock
            .Setup(x => x.AddPhotoToAlbumAsync(albumId, newPhotoId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _albumRepositoryMock.Verify(
            x => x.AddPhotoToAlbumAsync(albumId, newPhotoId, 2, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
