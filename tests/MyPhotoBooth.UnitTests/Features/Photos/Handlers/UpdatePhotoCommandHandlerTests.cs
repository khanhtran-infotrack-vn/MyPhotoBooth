using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Handlers;

public class UpdatePhotoCommandHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<UpdatePhotoCommandHandler>> _loggerMock;
    private readonly UpdatePhotoCommandHandler _handler;

    public UpdatePhotoCommandHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<UpdatePhotoCommandHandler>>();
        _handler = new UpdatePhotoCommandHandler(_photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_UpdatesPhotoDescription()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var newDescription = "Updated description";
        var command = new UpdatePhotoCommand(photoId, newDescription, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            Description = "Old description"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.Description.Should().Be(newDescription);
        _photoRepositoryMock.Verify(x => x.UpdateAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentPhoto_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new UpdatePhotoCommand(photoId, "Description", userId);

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
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new UpdatePhotoCommand(photoId, "Description", userId);

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
    public async Task Handle_NullDescription_SetsToNull()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new UpdatePhotoCommand(photoId, null, userId);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            Description = "Old description"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        photo.Description.Should().BeNull();
    }
}
