using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Queries;
using MyPhotoBooth.Application.Features.Albums.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Albums.Handlers;

public class GetAlbumQueryHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<GetAlbumQueryHandler>> _loggerMock;
    private readonly GetAlbumQueryHandler _handler;

    public GetAlbumQueryHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<GetAlbumQueryHandler>>();
        _handler = new GetAlbumQueryHandler(_albumRepositoryMock.Object, _photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_ReturnsAlbumDetails()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumQuery(albumId, userId);

        var photo1 = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "photo1.jpg",
            Width = 1920,
            Height = 1080,
            ThumbnailPath = "/thumbs/photo1.jpg"
        };

        var photo2 = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "photo2.jpg",
            Width = 1280,
            Height = 720,
            ThumbnailPath = "/thumbs/photo2.jpg"
        };

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            Name = "My Album",
            Description = "Test description",
            CoverPhotoId = photo1.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow.AddDays(-5),
            AlbumPhotos = new List<AlbumPhoto>
            {
                new AlbumPhoto { Photo = photo1, SortOrder = 1 },
                new AlbumPhoto { Photo = photo2, SortOrder = 0 }
            }
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        // Setup favorite status mock (no favorites for this test)
        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(albumId);
        result.Value.Name.Should().Be("My Album");
        result.Value.Description.Should().Be("Test description");
        result.Value.CoverPhotoId.Should().Be(photo1.Id);
        result.Value.Photos.Should().HaveCount(2);

        // Verify photos are ordered by SortOrder
        result.Value.Photos[0].Id.Should().Be(photo2.Id); // SortOrder 0
        result.Value.Photos[1].Id.Should().Be(photo1.Id); // SortOrder 1
    }

    [Fact]
    public async Task Handle_NonExistentAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumQuery(albumId, userId);

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Album?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Album not found");
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumQuery(albumId, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = "different-user-id"
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        // Setup favorite status mock (no favorites for this test)
        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
    }

    [Fact]
    public async Task Handle_EmptyAlbum_ReturnsEmptyPhotoList()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumQuery(albumId, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            Name = "Empty Album",
            AlbumPhotos = new List<AlbumPhoto>()
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        // Setup favorite status mock (no favorites for this test)
        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Photos.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullCapturedAt_PropagatesNullValue()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumQuery(albumId, userId);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "photo.jpg",
            Width = 1920,
            Height = 1080,
            CapturedAt = null,
            ThumbnailPath = "/thumbs/photo.jpg"
        };

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            AlbumPhotos = new List<AlbumPhoto>
            {
                new AlbumPhoto { Photo = photo, SortOrder = 0 }
            }
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        // Setup favorite status mock (no favorites for this test)
        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Photos[0].CapturedAt.Should().BeNull();
    }
}
