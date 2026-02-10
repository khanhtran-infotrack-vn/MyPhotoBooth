using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Features.ShareLinks.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.ShareLinks.Handlers;

public class ValidateShareLinkQueryHandlerTests
{
    private readonly Mock<IShareLinkRepository> _shareLinkRepositoryMock;
    private readonly Mock<ILogger<ValidateShareLinkQueryHandler>> _loggerMock;
    private readonly ValidateShareLinkQueryHandler _handler;

    public ValidateShareLinkQueryHandlerTests()
    {
        _shareLinkRepositoryMock = new Mock<IShareLinkRepository>();
        _loggerMock = new Mock<ILogger<ValidateShareLinkQueryHandler>>();
        _handler = new ValidateShareLinkQueryHandler(
            _shareLinkRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPhotoShare_ReturnsPhotoResponse()
    {
        // Arrange
        var token = "valid-photo-token";
        var query = new ValidateShareLinkQuery(token, null);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            PasswordHash = null,
            ExpiresAt = null,
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ShareLinkType.Photo);
        result.Value.Photo.Should().NotBeNull();
        result.Value.Photo.Id.Should().Be(photo.Id);
        result.Value.Album.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ValidAlbumShare_ReturnsAlbumResponse()
    {
        // Arrange
        var token = "valid-album-token";
        var query = new ValidateShareLinkQuery(token, null);

        var album = new Album
        {
            Id = Guid.NewGuid(),
            Name = "My Album",
            Description = "Test album"
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Album,
            AlbumId = album.Id,
            Album = album,
            PasswordHash = null,
            ExpiresAt = null,
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ShareLinkType.Album);
        result.Value.Album.Should().NotBeNull();
        result.Value.Album.Name.Should().Be(album.Name);
        result.Value.Photo.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NonExistentToken_ReturnsFailure()
    {
        // Arrange
        var token = "non-existent-token";
        var query = new ValidateShareLinkQuery(token, null);

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ShareLink?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Share link not found");
    }

    [Fact]
    public async Task Handle_RevokedLink_ReturnsFailure()
    {
        // Arrange
        var token = "revoked-token";
        var query = new ValidateShareLinkQuery(token, null);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Share link has been revoked");
    }

    [Fact]
    public async Task Handle_ExpiredLink_ReturnsFailure()
    {
        // Arrange
        var token = "expired-token";
        var query = new ValidateShareLinkQuery(token, null);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Share link has expired");
    }

    [Fact]
    public async Task Handle_LinkWithPassword_NoPasswordProvided_ReturnsFailure()
    {
        // Arrange
        var token = "protected-token";
        var query = new ValidateShareLinkQuery(token, null);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            PasswordHash = "hashed-password",
            ExpiresAt = null,
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Password required");
    }

    [Fact]
    public async Task Handle_LinkWithPassword_ValidPassword_ReturnsSuccess()
    {
        // Arrange
        var token = "protected-token";
        var password = "CorrectPassword123!";
        var query = new ValidateShareLinkQuery(token, password);

        var passwordHasher = new PasswordHasher<object>();
        var hashedPassword = passwordHasher.HashPassword(new object(), password);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            PasswordHash = hashedPassword,
            ExpiresAt = null,
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(ShareLinkType.Photo);
        result.Value.Photo.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_LinkWithPassword_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var token = "protected-token";
        var wrongPassword = "WrongPassword123!";
        var query = new ValidateShareLinkQuery(token, wrongPassword);

        var passwordHasher = new PasswordHasher<object>();
        var correctPasswordHash = passwordHasher.HashPassword(new object(), "CorrectPassword123!");

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            PasswordHash = correctPasswordHash,
            ExpiresAt = null,
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Incorrect password");
    }

    [Fact]
    public async Task Handle_ExpiredAndRevoked_ReturnsRevokedError()
    {
        // Arrange
        var token = "doubly-invalid-token";
        var query = new ValidateShareLinkQuery(token, null);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            RevokedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Expired check now comes before revoked check
        result.Error.Should().Be("Share link has expired");
    }

    [Fact]
    public async Task Handle_ExpiresInFuture_ReturnsSuccess()
    {
        // Arrange
        var token = "future-expiry-token";
        var query = new ValidateShareLinkQuery(token, null);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg",
            FilePath = "/storage/test.jpg",
            ThumbnailPath = "/storage/thumb.jpg",
            ContentType = "image/jpeg",
            Width = 1920,
            Height = 1080
        };

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = photo.Id,
            Photo = photo,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLink);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
