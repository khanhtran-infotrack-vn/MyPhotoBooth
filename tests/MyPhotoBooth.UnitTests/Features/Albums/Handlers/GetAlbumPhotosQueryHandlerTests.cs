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

public class GetAlbumPhotosQueryHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<GetAlbumPhotosQueryHandler>> _loggerMock;
    private readonly GetAlbumPhotosQueryHandler _handler;

    public GetAlbumPhotosQueryHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<GetAlbumPhotosQueryHandler>>();
        _handler = new GetAlbumPhotosQueryHandler(_albumRepositoryMock.Object, _photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_ReturnsAlbumPhotos()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumPhotosQuery(albumId, userId);

        var photo1 = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "photo1.jpg",
            Width = 1920,
            Height = 1080,
            CapturedAt = new DateTime(2024, 1, 15),
            UploadedAt = DateTime.UtcNow,
            ThumbnailPath = "/thumbs/photo1.jpg"
        };

        var photo2 = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "photo2.jpg",
            Width = 1280,
            Height = 720,
            CapturedAt = new DateTime(2024, 1, 16),
            UploadedAt = DateTime.UtcNow,
            ThumbnailPath = "/thumbs/photo2.jpg"
        };

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
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
        result.Value.Should().HaveCount(2);

        // Verify ordering by SortOrder
        result.Value[0].Id.Should().Be(photo2.Id); // SortOrder 0
        result.Value[1].Id.Should().Be(photo1.Id); // SortOrder 1

        result.Value[0].OriginalFileName.Should().Be("photo2.jpg");
        result.Value[1].OriginalFileName.Should().Be("photo1.jpg");
    }

    [Fact]
    public async Task Handle_NonExistentAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumPhotosQuery(albumId, userId);

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
        var query = new GetAlbumPhotosQuery(albumId, userId);

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
    public async Task Handle_EmptyAlbum_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumPhotosQuery(albumId, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
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
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullCapturedAt_PropagatesNullValue()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumPhotosQuery(albumId, userId);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "photo.jpg",
            Width = 1920,
            Height = 1080,
            CapturedAt = null,
            UploadedAt = DateTime.UtcNow,
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
        result.Value[0].CapturedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_LargeAlbum_HandlesManyPhotos()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var query = new GetAlbumPhotosQuery(albumId, userId);

        var albumPhotos = Enumerable.Range(0, 100)
            .Select(i => new AlbumPhoto
            {
                Photo = new Photo
                {
                    Id = Guid.NewGuid(),
                    OriginalFileName = $"photo{i}.jpg",
                    Width = 1920,
                    Height = 1080,
                    ThumbnailPath = $"/thumbs/photo{i}.jpg"
                },
                SortOrder = i
            })
            .ToList();

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            AlbumPhotos = albumPhotos
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
        result.Value.Should().HaveCount(100);

        // Verify correct ordering
        for (int i = 0; i < 100; i++)
        {
            result.Value[i].OriginalFileName.Should().Be($"photo{i}.jpg");
        }
    }
}
