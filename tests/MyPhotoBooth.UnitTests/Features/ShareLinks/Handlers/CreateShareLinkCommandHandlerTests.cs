using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Application.Features.ShareLinks.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.ShareLinks.Handlers;

public class CreateShareLinkCommandHandlerTests
{
    private readonly Mock<IShareLinkRepository> _shareLinkRepositoryMock;
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<ILogger<CreateShareLinkCommandHandler>> _loggerMock;
    private readonly CreateShareLinkCommandHandler _handler;

    public CreateShareLinkCommandHandlerTests()
    {
        _shareLinkRepositoryMock = new Mock<IShareLinkRepository>();
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _loggerMock = new Mock<ILogger<CreateShareLinkCommandHandler>>();
        _handler = new CreateShareLinkCommandHandler(
            _shareLinkRepositoryMock.Object,
            _photoRepositoryMock.Object,
            _albumRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_PhotoShare_ValidOwner_CreatesShareLink()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var baseUrl = "https://example.com";
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            photoId,
            null,
            null,
            true,
            null,
            userId,
            baseUrl);

        var photo = new Photo
        {
            Id = photoId,
            UserId = userId,
            OriginalFileName = "test.jpg"
        };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        _shareLinkRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShareLink sl, CancellationToken _) => sl);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ShareLinkType.Photo);
        result.Value.PhotoId.Should().Be(photoId);
        result.Value.HasPassword.Should().BeFalse();
        result.Value.AllowDownload.Should().BeTrue();
        result.Value.ShareUrl.Should().StartWith($"{baseUrl}/shared/");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AlbumShare_ValidOwner_CreatesShareLink()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var baseUrl = "https://example.com";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var command = new CreateShareLinkCommand(
            ShareLinkType.Album,
            null,
            albumId,
            expiresAt,
            false,
            null,
            userId,
            baseUrl);

        var album = new Album
        {
            Id = albumId,
            UserId = userId,
            Name = "My Album"
        };

        _albumRepositoryMock
            .Setup(x => x.GetByIdAsync(albumId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);

        _shareLinkRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShareLink sl, CancellationToken _) => sl);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ShareLinkType.Album);
        result.Value.AlbumId.Should().Be(albumId);
        result.Value.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
        result.Value.AllowDownload.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithPassword_HashesPassword()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var password = "SecurePassword123!";
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            photoId,
            null,
            null,
            true,
            password,
            userId,
            "https://example.com");

        var photo = new Photo { Id = photoId, UserId = userId };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        ShareLink? capturedShareLink = null;
        _shareLinkRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Callback<ShareLink, CancellationToken>((sl, _) => capturedShareLink = sl)
            .ReturnsAsync((ShareLink sl, CancellationToken _) => sl);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedShareLink.Should().NotBeNull();
        capturedShareLink!.PasswordHash.Should().NotBeNull();
        capturedShareLink.PasswordHash.Should().NotBe(password);
        result.Value.HasPassword.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_PhotoNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            photoId,
            null,
            null,
            true,
            null,
            userId,
            "https://example.com");

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
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            photoId,
            null,
            null,
            true,
            null,
            userId,
            "https://example.com");

        var photo = new Photo { Id = photoId, UserId = "different-user-id" };

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
    public async Task Handle_AlbumNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new CreateShareLinkCommand(
            ShareLinkType.Album,
            null,
            albumId,
            null,
            true,
            null,
            userId,
            "https://example.com");

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
    public async Task Handle_UnauthorizedAlbum_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var albumId = Guid.NewGuid();
        var command = new CreateShareLinkCommand(
            ShareLinkType.Album,
            null,
            albumId,
            null,
            true,
            null,
            userId,
            "https://example.com");

        var album = new Album { Id = albumId, UserId = "different-user-id" };

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
    public async Task Handle_GeneratesUniqueToken()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            photoId,
            null,
            null,
            true,
            null,
            userId,
            "https://example.com");

        var photo = new Photo { Id = photoId, UserId = userId };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        ShareLink? capturedShareLink = null;
        _shareLinkRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Callback<ShareLink, CancellationToken>((sl, _) => capturedShareLink = sl)
            .ReturnsAsync((ShareLink sl, CancellationToken _) => sl);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedShareLink.Should().NotBeNull();
        capturedShareLink!.Token.Should().NotBeEmpty();
        result.Value.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_SetsCreatedAtTimestamp()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            photoId,
            null,
            null,
            true,
            null,
            userId,
            "https://example.com");

        var photo = new Photo { Id = photoId, UserId = userId };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        ShareLink? capturedShareLink = null;
        _shareLinkRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Callback<ShareLink, CancellationToken>((sl, _) => capturedShareLink = sl)
            .ReturnsAsync((ShareLink sl, CancellationToken _) => sl);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedShareLink.Should().NotBeNull();
        capturedShareLink!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_ConvertsExpiresAtToUtc()
    {
        // Arrange
        var userId = "user-id";
        var photoId = Guid.NewGuid();
        var localExpiresAt = DateTime.Now.AddDays(7);
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            photoId,
            null,
            localExpiresAt,
            true,
            null,
            userId,
            "https://example.com");

        var photo = new Photo { Id = photoId, UserId = userId };

        _photoRepositoryMock
            .Setup(x => x.GetByIdAsync(photoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        ShareLink? capturedShareLink = null;
        _shareLinkRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ShareLink>(), It.IsAny<CancellationToken>()))
            .Callback<ShareLink, CancellationToken>((sl, _) => capturedShareLink = sl)
            .ReturnsAsync((ShareLink sl, CancellationToken _) => sl);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedShareLink.Should().NotBeNull();
        capturedShareLink!.ExpiresAt.Should().BeCloseTo(localExpiresAt.ToUniversalTime(), TimeSpan.FromSeconds(1));
    }
}
