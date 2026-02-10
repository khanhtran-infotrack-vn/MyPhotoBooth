using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Photos.Handlers;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Handlers;

public class GetFavoritePhotosQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<GetFavoritePhotosQueryHandler>> _loggerMock;
    private readonly GetFavoritePhotosQueryHandler _handler;

    public GetFavoritePhotosQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<GetFavoritePhotosQueryHandler>>();
        _handler = new GetFavoritePhotosQueryHandler(
            _photoRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsPaginatedFavorites_WhenUserHasFavorites()
    {
        // Arrange
        var userId = "user-id";
        var photos = new List<Photo>();

        for (int i = 0; i < 5; i++)
        {
            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OriginalFileName = $"photo{i}.jpg",
                FilePath = $"/path/to/file{i}",
                ThumbnailPath = $"/path/to/thumb{i}",
                ContentType = "image/jpeg",
                FileSize = 1000,
                Width = 1920,
                Height = 1080,
                UploadedAt = DateTime.UtcNow.AddDays(-i)
            };
            photos.Add(photo);
        }

        _photoRepositoryMock
            .Setup(x => x.GetFavoritesAsync(userId, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(x => x.GetFavoritesCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var query = new GetFavoritePhotosQuery(userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(5);
        result.Value.Page.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenUserHasNoFavorites()
    {
        // Arrange
        var userId = "user-id";
        _photoRepositoryMock
            .Setup(x => x.GetFavoritesAsync(userId, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Photo>());

        _photoRepositoryMock
            .Setup(x => x.GetFavoritesCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetFavoritePhotosQuery(userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPage_WhenUsingPagination()
    {
        // Arrange
        var userId = "user-id";
        var photos = new List<Photo>();

        for (int i = 0; i < 5; i++)
        {
            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OriginalFileName = $"photo{i}.jpg",
                FilePath = $"/path/to/file{i}",
                ThumbnailPath = $"/path/to/thumb{i}",
                ContentType = "image/jpeg",
                FileSize = 1000,
                Width = 1920,
                Height = 1080,
                UploadedAt = DateTime.UtcNow.AddDays(-i)
            };
            photos.Add(photo);
        }

        _photoRepositoryMock
            .Setup(x => x.GetFavoritesAsync(userId, 5, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(x => x.GetFavoritesCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        var query = new GetFavoritePhotosQuery(userId, 2, 5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(15);
        result.Value.Page.Should().Be(2);
        result.Value.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_OnlyReturnsUserFavorites_WhenMultipleUsersExist()
    {
        // Arrange
        var user1Id = "user-1";
        var user2Id = "user-2";

        var photo1 = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = user1Id,
            OriginalFileName = "photo1.jpg",
            FilePath = "/path/to/file1",
            ThumbnailPath = "/path/to/thumb1",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        var photo2 = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = user2Id,
            OriginalFileName = "photo2.jpg",
            FilePath = "/path/to/file2",
            ThumbnailPath = "/path/to/thumb2",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.GetFavoritesAsync(user1Id, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { photo1 });

        _photoRepositoryMock
            .Setup(x => x.GetFavoritesCountAsync(user1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new GetFavoritePhotosQuery(user1Id, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Items[0].Id.Should().Be(photo1.Id);
    }
}
