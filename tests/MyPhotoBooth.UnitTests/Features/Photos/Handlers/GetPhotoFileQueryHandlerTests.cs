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

public class GetPhotoFileQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<GetPhotoFileQueryHandler>> _loggerMock;
    private readonly GetPhotoFileQueryHandler _handler;

    public GetPhotoFileQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _loggerMock = new Mock<ILogger<GetPhotoFileQueryHandler>>();
        _handler = new GetPhotoFileQueryHandler(
            _photoRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_ReturnsPhotoFile()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoFileQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            FilePath = "/storage/original.jpg",
            ContentType = "image/jpeg",
            OriginalFileName = "test.jpg"
        };

        var fileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _fileStorageServiceMock
            .Setup(x => x.GetFileStreamAsync(photo.FilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Stream.Should().NotBeNull();
        result.Value.ContentType.Should().Be("image/jpeg");
        result.Value.FileName.Should().Be("test.jpg");
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoFileQuery(photoId, userId);

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
        var query = new GetPhotoFileQuery(photoId, userId);

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
    public async Task Handle_StorageError_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var query = new GetPhotoFileQuery(photoId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            FilePath = "/storage/original.jpg",
            ContentType = "image/jpeg",
            OriginalFileName = "test.jpg"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _fileStorageServiceMock
            .Setup(x => x.GetFileStreamAsync(photo.FilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Failed to store file");
    }
}
