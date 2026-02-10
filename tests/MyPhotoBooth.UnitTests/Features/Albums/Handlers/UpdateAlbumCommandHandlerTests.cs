using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Features.Albums.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Albums.Handlers;

public class UpdateAlbumCommandHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<ILogger<UpdateAlbumCommandHandler>> _loggerMock;
    private readonly UpdateAlbumCommandHandler _handler;

    public UpdateAlbumCommandHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _loggerMock = new Mock<ILogger<UpdateAlbumCommandHandler>>();
        _handler = new UpdateAlbumCommandHandler(_albumRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_UpdatesAlbumSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var newName = "Updated Album Name";
        var newDescription = "Updated description";
        var coverPhotoId = Guid.NewGuid();
        var command = new UpdateAlbumCommand(albumId, newName, newDescription, coverPhotoId, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            Name = "Old Name",
            Description = "Old Description",
            CoverPhotoId = null
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _albumRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        album.Name.Should().Be(newName);
        album.Description.Should().Be(newDescription);
        album.CoverPhotoId.Should().Be(coverPhotoId);
        album.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _albumRepositoryMock.Verify(x => x.UpdateAsync(album, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new UpdateAlbumCommand(albumId, "Name", "Description", null, userId);

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Album?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

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
        var command = new UpdateAlbumCommand(albumId, "Name", "Description", null, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = "different-user-id"
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
    }

    [Fact]
    public async Task Handle_WithNullCoverPhotoId_UpdatesToNull()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new UpdateAlbumCommand(albumId, "Name", "Description", null, userId);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            Name = "Old Name",
            CoverPhotoId = Guid.NewGuid()
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _albumRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        album.CoverPhotoId.Should().BeNull();
    }
}
