using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Features.Photos.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Handlers;

public class GetPhotoQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<GetPhotoQueryHandler>> _loggerMock;
    private readonly GetPhotoQueryHandler _handler;

    public GetPhotoQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        // Setup default behavior for IsFavoriteAsync
        _photoRepositoryMock
            .Setup(x => x.IsFavoriteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _loggerMock = new Mock<ILogger<GetPhotoQueryHandler>>();
        _handler = new GetPhotoQueryHandler(_photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_ReturnsPhotoDetails()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoQuery(photoId, userId);

        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "nature" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "sunset" };

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg",
            FileSize = 1024 * 1024,
            Width = 1920,
            Height = 1080,
            Description = "Beautiful sunset",
            ExifDataJson = "{\"Camera\": \"Canon\"}",
            UploadedAt = DateTime.UtcNow,
            PhotoTags = new List<PhotoTag>
            {
                new PhotoTag { Tag = tag1 },
                new PhotoTag { Tag = tag2 }
            }
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(photoId);
        result.Value.OriginalFileName.Should().Be("test.jpg");
        result.Value.Width.Should().Be(1920);
        result.Value.Height.Should().Be(1080);
        result.Value.Description.Should().Be("Beautiful sunset");
        result.Value.ExifData.Should().Be("{\"Camera\": \"Canon\"}");
        result.Value.Tags.Should().BeEquivalentTo(new[] { "nature", "sunset" });
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoQuery(photoId, userId);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Photo?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Photo not found");
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = "different-user-id"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
    }

    [Fact]
    public async Task Handle_PhotoWithNoTags_ReturnsEmptyTagList()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg",
            FileSize = 1024,
            Width = 1920,
            Height = 1080,
            PhotoTags = new List<PhotoTag>()
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PhotoWithNullCapturedAt_StillReturnsSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg",
            FileSize = 1024,
            Width = 1920,
            Height = 1080,
            CapturedAt = null,
            UploadedAt = DateTime.UtcNow,
            PhotoTags = new List<PhotoTag>()
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CapturedAt.Should().BeNull();
    }
}
