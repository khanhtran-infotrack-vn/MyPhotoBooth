using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Features.Albums.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Albums.Handlers;

public class CreateAlbumCommandHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<ILogger<CreateAlbumCommandHandler>> _loggerMock;
    private readonly CreateAlbumCommandHandler _handler;

    public CreateAlbumCommandHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _loggerMock = new Mock<ILogger<CreateAlbumCommandHandler>>();
        _handler = new CreateAlbumCommandHandler(_albumRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAlbumSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var name = "My Vacation";
        var description = "Summer 2024 trip";
        var command = new CreateAlbumCommand(name, description, userId);

        Album? capturedAlbum = null;
        _albumRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()))
            .Callback<Album, CancellationToken>((album, _) => capturedAlbum = album)
            .ReturnsAsync((Album album, CancellationToken _) => album);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.PhotoCount.Should().Be(0);
        result.Value.CoverPhotoId.Should().BeNull();

        _albumRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Album>(a =>
                a.Name == name &&
                a.Description == description &&
                a.UserId == userId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullDescription_CreatesAlbumSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var name = "My Album";
        var command = new CreateAlbumCommand(name, null, userId);

        Album? capturedAlbum = null;
        _albumRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()))
            .Callback<Album, CancellationToken>((album, _) => capturedAlbum = album)
            .ReturnsAsync((Album album, CancellationToken _) => album);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SetsTimestampsCorrectly()
    {
        // Arrange
        var userId = "user-id";
        var command = new CreateAlbumCommand("Test Album", "Description", userId);

        Album? capturedAlbum = null;
        _albumRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()))
            .Callback<Album, CancellationToken>((album, _) => capturedAlbum = album)
            .Returns((Album album, CancellationToken _) => Task.FromResult(album));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedAlbum.Should().NotBeNull();
        capturedAlbum!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        capturedAlbum.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_GeneratesUniqueId()
    {
        // Arrange
        var userId = "user-id";
        var command = new CreateAlbumCommand("Test Album", null, userId);

        Album? capturedAlbum = null;
        _albumRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()))
            .Callback<Album, CancellationToken>((album, _) => capturedAlbum = album)
            .Returns((Album album, CancellationToken _) => Task.FromResult(album));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedAlbum.Should().NotBeNull();
        capturedAlbum!.Id.Should().NotBeEmpty();
        result.Value.Id.Should().NotBeEmpty();
    }
}
