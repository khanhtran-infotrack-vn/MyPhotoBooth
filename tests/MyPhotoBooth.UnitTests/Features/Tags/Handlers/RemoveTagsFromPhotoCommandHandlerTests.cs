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

public class RemoveTagsFromPhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<RemoveTagsFromPhotoCommandHandler>> _loggerMock;
    private readonly RemoveTagsFromPhotoCommandHandler _handler;

    public RemoveTagsFromPhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<RemoveTagsFromPhotoCommandHandler>>();
        _handler = new RemoveTagsFromPhotoCommandHandler(
            _photoRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_RemovesTagsSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tag1Id = Guid.NewGuid();
        var tag2Id = Guid.NewGuid();
        var command = new RemoveTagsFromPhotoCommand(photoId, new[] { tag1Id, tag2Id }.ToList(), userId);

        var tag1 = new Tag { Id = tag1Id, Name = "nature" };
        var tag2 = new Tag { Id = tag2Id, Name = "sunset" };

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>
            {
                new PhotoTag { PhotoId = photoId, TagId = tag1Id, Tag = tag1 },
                new PhotoTag { PhotoId = photoId, TagId = tag2Id, Tag = tag2 }
            }
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagIds = new[] { Guid.NewGuid() };
        var command = new RemoveTagsFromPhotoCommand(photoId, tagIds.ToList(), userId);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
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
        var photoId = Guid.NewGuid();
        var tagIds = new[] { Guid.NewGuid() };
        var command = new RemoveTagsFromPhotoCommand(photoId, tagIds.ToList(), userId);

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
    }

    [Fact]
    public async Task Handle_NonExistentTagOnPhoto_SkipsRemoval()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new RemoveTagsFromPhotoCommand(photoId, new[] { tagId }.ToList(), userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>()
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PartialTagRemoval_RemovesOnlySpecifiedTags()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tag1Id = Guid.NewGuid();
        var tag2Id = Guid.NewGuid();
        var tag3Id = Guid.NewGuid();
        var command = new RemoveTagsFromPhotoCommand(photoId, new[] { tag1Id, tag2Id }.ToList(), userId);

        var tag1 = new Tag { Id = tag1Id, Name = "nature" };
        var tag2 = new Tag { Id = tag2Id, Name = "sunset" };
        var tag3 = new Tag { Id = tag3Id, Name = "beach" };

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>
            {
                new PhotoTag { PhotoId = photoId, TagId = tag1Id, Tag = tag1 },
                new PhotoTag { PhotoId = photoId, TagId = tag2Id, Tag = tag2 },
                new PhotoTag { PhotoId = photoId, TagId = tag3Id, Tag = tag3 }
            }
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().HaveCount(1);
        photo.PhotoTags.ToList()[0].TagId.Should().Be(tag3Id);
    }

    [Fact]
    public async Task Handle_EmptyTagList_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new RemoveTagsFromPhotoCommand(photoId, Array.Empty<Guid>().ToList(), userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>()
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RemovesAllTagsWhenRequested()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tag1Id = Guid.NewGuid();
        var tag2Id = Guid.NewGuid();

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>
            {
                new PhotoTag { PhotoId = photoId, TagId = tag1Id, Tag = new Tag { Id = tag1Id } },
                new PhotoTag { PhotoId = photoId, TagId = tag2Id, Tag = new Tag { Id = tag2Id } }
            }
        };

        var command = new RemoveTagsFromPhotoCommand(photoId, new[] { tag1Id, tag2Id }.ToList(), userId);

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().BeEmpty();
    }
}
