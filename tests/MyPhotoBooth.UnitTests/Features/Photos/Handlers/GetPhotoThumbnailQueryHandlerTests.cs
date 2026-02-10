using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Features.Photos.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Handlers;

public class GetPhotoThumbnailQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<GetPhotoThumbnailQueryHandler>> _loggerMock;
    private readonly GetPhotoThumbnailQueryHandler _handler;

    public GetPhotoThumbnailQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<GetPhotoThumbnailQueryHandler>>();
        _handler = new GetPhotoThumbnailQueryHandler(
            _photoRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_ReturnsThumbnailStream()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoThumbnailQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            ThumbnailPath = "/storage/thumb.jpg"
        };

        var thumbnailStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _fileStorageServiceMock
            .Setup(x => x.GetFileStreamAsync(photo.ThumbnailPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(thumbnailStream);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoThumbnailQuery(photoId, userId);

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
        var query = new GetPhotoThumbnailQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = "different-user-id",
            ThumbnailPath = "/storage/thumb.jpg"
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
    public async Task Handle_StorageError_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoThumbnailQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            ThumbnailPath = "/storage/thumb.jpg"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _fileStorageServiceMock
            .Setup(x => x.GetFileStreamAsync(photo.ThumbnailPath, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Failed to store file");
    }

    [Fact]
    public async Task Handle_NullUserId_ReturnsFailure()
    {
        // Arrange
        string? nullUserId = null;
        var photoId = Guid.NewGuid();
        var query = new GetPhotoThumbnailQuery(photoId, nullUserId!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // The handler passes null to the repository, which returns null, resulting in "Photo not found"
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Photo not found");
    }
}
