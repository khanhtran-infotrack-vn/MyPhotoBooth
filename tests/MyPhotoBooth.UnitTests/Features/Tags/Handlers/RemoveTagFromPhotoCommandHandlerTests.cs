using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Features.Tags.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Handlers;

public class RemoveTagFromPhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<RemoveTagFromPhotoCommandHandler>> _loggerMock;
    private readonly RemoveTagFromPhotoCommandHandler _handler;

    public RemoveTagFromPhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<RemoveTagFromPhotoCommandHandler>>();
        _handler = new RemoveTagFromPhotoCommandHandler(_photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Removes_Tag_From_Photo_Successfully()
    {
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new RemoveTagFromPhotoCommand(photoId, tagId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg",
            FilePath = "/path/to/test.jpg",
            ThumbnailPath = "/path/to/thumb.jpg",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        var photoTag = new PhotoTag
        {
            PhotoId = photoId,
            TagId = tagId,
            Photo = photo,
            Tag = new Tag { Id = tagId, Name = "test-tag", UserId = userId }
        };

        photo.PhotoTags.Add(photoTag);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().BeEmpty();
        _photoRepositoryMock.Verify(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Returns_Error_When_Photo_Not_Found()
    {
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new RemoveTagFromPhotoCommand(photoId, tagId, userId);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Photo?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Photo not found");
    }

    [Fact]
    public async Task Handle_Returns_Error_When_User_Not_Authorized()
    {
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new RemoveTagFromPhotoCommand(photoId, tagId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = "different-user-id",
            OriginalFileName = "test.jpg",
            FilePath = "/path/to/test.jpg",
            ThumbnailPath = "/path/to/thumb.jpg",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
    }

    [Fact]
    public async Task Handle_Succeeds_When_Tag_Not_Assigned_To_Photo()
    {
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new RemoveTagFromPhotoCommand(photoId, tagId, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg",
            FilePath = "/path/to/test.jpg",
            ThumbnailPath = "/path/to/thumb.jpg",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _photoRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Photo>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
