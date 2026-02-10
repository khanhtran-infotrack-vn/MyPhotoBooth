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

public class SearchPhotosQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<SearchPhotosQueryHandler>> _loggerMock;
    private readonly SearchPhotosQueryHandler _handler;
    private readonly string _userId;

    public SearchPhotosQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<SearchPhotosQueryHandler>>();
        _handler = new SearchPhotosQueryHandler(
            _photoRepositoryMock.Object,
            _loggerMock.Object);
        _userId = "user-id";
    }

    [Fact]
    public async Task Handle_ReturnsPhotos_WhenSearchMatchesFilename()
    {
        // Arrange
        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            OriginalFileName = "vacation-photo.jpg",
            FilePath = "/path/to/file",
            ThumbnailPath = "/path/to/thumb",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.SearchAsync(_userId, "vacation", 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { photo });

        _photoRepositoryMock
            .Setup(x => x.GetSearchCountAsync(_userId, "vacation", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new SearchPhotosQuery("vacation", _userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].OriginalFileName.Should().Be("vacation-photo.jpg");
    }

    [Fact]
    public async Task Handle_ReturnsPhotos_WhenSearchMatchesDescription()
    {
        // Arrange
        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            OriginalFileName = "photo.jpg",
            Description = "Beautiful sunset at the beach",
            FilePath = "/path/to/file",
            ThumbnailPath = "/path/to/thumb",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.SearchAsync(_userId, "sunset", 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { photo });

        _photoRepositoryMock
            .Setup(x => x.GetSearchCountAsync(_userId, "sunset", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new SearchPhotosQuery("sunset", _userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Id.Should().Be(photo.Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoMatchesFound()
    {
        // Arrange
        _photoRepositoryMock
            .Setup(x => x.SearchAsync(_userId, "nonexistentterm", 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Photo>());

        _photoRepositoryMock
            .Setup(x => x.GetSearchCountAsync(_userId, "nonexistentterm", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new SearchPhotosQuery("nonexistentterm", _userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_OnlyReturnsUserPhotos_WhenMultipleUsersExist()
    {
        // Arrange
        var otherUserId = "other-user";

        var photo1 = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            OriginalFileName = "vacation.jpg",
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
            UserId = otherUserId,
            OriginalFileName = "vacation.jpg",
            FilePath = "/path/to/file2",
            ThumbnailPath = "/path/to/thumb2",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.SearchAsync(_userId, "vacation", 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { photo1 });

        _photoRepositoryMock
            .Setup(x => x.GetSearchCountAsync(_userId, "vacation", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new SearchPhotosQuery("vacation", _userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Id.Should().Be(photo1.Id);
    }

    [Fact]
    public async Task Handle_IsCaseInsensitive_WhenSearching()
    {
        // Arrange
        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = _userId,
            OriginalFileName = "VACATION-PHOTO.jpg",
            Description = "Beautiful SUNSET",
            FilePath = "/path/to/file",
            ThumbnailPath = "/path/to/thumb",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.SearchAsync(_userId, "vacation sunset", 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { photo });

        _photoRepositoryMock
            .Setup(x => x.GetSearchCountAsync(_userId, "vacation sunset", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new SearchPhotosQuery("vacation sunset", _userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }
}
