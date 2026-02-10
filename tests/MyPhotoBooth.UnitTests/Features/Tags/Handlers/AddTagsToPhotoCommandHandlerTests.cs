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

public class AddTagsToPhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<ILogger<AddTagsToPhotoCommandHandler>> _loggerMock;
    private readonly AddTagsToPhotoCommandHandler _handler;

    public AddTagsToPhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _tagRepositoryMock = new Mock<ITagRepository>();
        _loggerMock = new Mock<ILogger<AddTagsToPhotoCommandHandler>>();
        _handler = new AddTagsToPhotoCommandHandler(
            _photoRepositoryMock.Object,
            _tagRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_AddsTagsSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var command = new AddTagsToPhotoCommand(photoId, tagIds.ToList(), userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>()
        };

        var tag1 = new Tag { Id = tagIds[0], UserId = userId, Name = "nature" };
        var tag2 = new Tag { Id = tagIds[1], UserId = userId, Name = "sunset" };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagIds[0], It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag1);

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagIds[1], It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag2);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().HaveCount(2);
        photo.PhotoTags.ToList()[0].TagId.Should().Be(tagIds[0]);
        photo.PhotoTags.ToList()[1].TagId.Should().Be(tagIds[1]);
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagIds = new[] { Guid.NewGuid() };
        var command = new AddTagsToPhotoCommand(photoId, tagIds.ToList(), userId);

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
        var command = new AddTagsToPhotoCommand(photoId, tagIds.ToList(), userId);

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
    public async Task Handle_NonExistentTag_SkipsTagAndContinues()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new AddTagsToPhotoCommand(photoId, new[] { tagId }.ToList(), userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>()
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

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
    public async Task Handle_UnauthorizedTag_SkipsTagAndContinues()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new AddTagsToPhotoCommand(photoId, new[] { tagId }.ToList(), userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>()
        };

        var tag = new Tag
        {
            Id = tagId,
            UserId = "different-user-id"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

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
    public async Task Handle_AlreadyExistingTag_SkipsAdding()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var command = new AddTagsToPhotoCommand(photoId, new[] { tagId }.ToList(), userId);

        var tag = new Tag { Id = tagId, UserId = userId, Name = "nature" };

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            PhotoTags = new List<PhotoTag>
            {
                new PhotoTag { PhotoId = photoId, TagId = tagId, Tag = tag }
            }
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _photoRepositoryMock
            .Setup(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.PhotoTags.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_EmptyTagList_ReturnsSuccess()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new AddTagsToPhotoCommand(photoId, Array.Empty<Guid>().ToList(), userId);

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
        _tagRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
