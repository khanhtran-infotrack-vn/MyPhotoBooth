using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.ShareLinks.Queries;
using MyPhotoBooth.Application.Features.ShareLinks.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.ShareLinks.Handlers;

public class GetShareLinksQueryHandlerTests
{
    private readonly Mock<IShareLinkRepository> _shareLinkRepositoryMock;
    private readonly Mock<ILogger<GetShareLinksQueryHandler>> _loggerMock;
    private readonly GetShareLinksQueryHandler _handler;

    public GetShareLinksQueryHandlerTests()
    {
        _shareLinkRepositoryMock = new Mock<IShareLinkRepository>();
        _loggerMock = new Mock<ILogger<GetShareLinksQueryHandler>>();
        _handler = new GetShareLinksQueryHandler(
            _shareLinkRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsShareLinks()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://example.com";
        var query = new GetShareLinksQuery(userId, baseUrl);

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = "test.jpg"
        };

        var album = new Album
        {
            Id = Guid.NewGuid(),
            Name = "My Album"
        };

        var shareLinks = new List<ShareLink>
        {
            new ShareLink
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = "abc123",
                Type = ShareLinkType.Photo,
                PhotoId = photo.Id,
                Photo = photo,
                PasswordHash = null,
                ExpiresAt = null,
                AllowDownload = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                RevokedAt = null
            },
            new ShareLink
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = "def456",
                Type = ShareLinkType.Album,
                AlbumId = album.Id,
                Album = album,
                PasswordHash = "hashed-password",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                AllowDownload = false,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                RevokedAt = null
            }
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLinks);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        // First share link (photo)
        result.Value[0].Type.Should().Be(ShareLinkType.Photo);
        result.Value[0].Token.Should().Be("abc123");
        result.Value[0].PhotoId.Should().Be(photo.Id);
        result.Value[0].AlbumId.Should().BeNull();
        result.Value[0].TargetName.Should().Be("test.jpg");
        result.Value[0].HasPassword.Should().BeFalse();
        result.Value[0].ExpiresAt.Should().BeNull();
        result.Value[0].AllowDownload.Should().BeTrue();
        result.Value[0].ShareUrl.Should().Be($"{baseUrl}/shared/abc123");
        result.Value[0].IsActive.Should().BeTrue();

        // Second share link (album)
        result.Value[1].Type.Should().Be(ShareLinkType.Album);
        result.Value[1].Token.Should().Be("def456");
        result.Value[1].AlbumId.Should().Be(album.Id);
        result.Value[1].PhotoId.Should().BeNull();
        result.Value[1].TargetName.Should().Be("My Album");
        result.Value[1].HasPassword.Should().BeTrue();
        result.Value[1].ExpiresAt.Should().BeCloseTo(shareLinks[1].ExpiresAt!.Value, TimeSpan.FromSeconds(1));
        result.Value[1].AllowDownload.Should().BeFalse();
        result.Value[1].ShareUrl.Should().Be($"{baseUrl}/shared/def456");
        result.Value[1].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetShareLinksQuery(userId, "https://example.com");

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShareLink>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RevokedShareLink_IsActiveIsFalse()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://example.com";
        var query = new GetShareLinksQuery(userId, baseUrl);

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "abc123",
            Type = ShareLinkType.Photo,
            PhotoId = Guid.NewGuid(),
            Photo = new Photo { OriginalFileName = "test.jpg" },
            RevokedAt = DateTime.UtcNow.AddHours(-1)
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShareLink> { shareLink });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExpiredShareLink_IsActiveIsFalse()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://example.com";
        var query = new GetShareLinksQuery(userId, baseUrl);

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "abc123",
            Type = ShareLinkType.Photo,
            PhotoId = Guid.NewGuid(),
            Photo = new Photo { OriginalFileName = "test.jpg" },
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            RevokedAt = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShareLink> { shareLink });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PhotoShareWithoutPhoto_TargetNameIsNull()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://example.com";
        var query = new GetShareLinksQuery(userId, baseUrl);

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "abc123",
            Type = ShareLinkType.Photo,
            PhotoId = Guid.NewGuid(),
            Photo = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShareLink> { shareLink });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].TargetName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AlbumShareWithoutAlbum_TargetNameIsNull()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://example.com";
        var query = new GetShareLinksQuery(userId, baseUrl);

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "abc123",
            Type = ShareLinkType.Album,
            AlbumId = Guid.NewGuid(),
            Album = null
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShareLink> { shareLink });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].TargetName.Should().BeNull();
    }

    [Fact]
    public async Task Handle_PreservesTimestamps()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://example.com";
        var createdAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var expiresAt = new DateTime(2024, 2, 1, 10, 0, 0, DateTimeKind.Utc);
        var query = new GetShareLinksQuery(userId, baseUrl);

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "abc123",
            Type = ShareLinkType.Photo,
            PhotoId = Guid.NewGuid(),
            Photo = new Photo { OriginalFileName = "test.jpg" },
            CreatedAt = createdAt,
            ExpiresAt = expiresAt
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShareLink> { shareLink });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].CreatedAt.Should().Be(createdAt);
        result.Value[0].ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public async Task Handle_ConstructsCorrectShareUrl()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://myapp.com";
        var token = "custom-token-123";
        var query = new GetShareLinksQuery(userId, baseUrl);

        var shareLink = new ShareLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            Type = ShareLinkType.Photo,
            PhotoId = Guid.NewGuid(),
            Photo = new Photo { OriginalFileName = "test.jpg" }
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ShareLink> { shareLink });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].ShareUrl.Should().Be($"{baseUrl}/shared/{token}");
    }

    [Fact]
    public async Task Handle_MultipleShareLinks_ReturnsAllForUser()
    {
        // Arrange
        var userId = "user-id";
        var baseUrl = "https://example.com";
        var query = new GetShareLinksQuery(userId, baseUrl);

        var shareLinks = Enumerable.Range(1, 20)
            .Select(i => new ShareLink
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = $"token{i}",
                Type = ShareLinkType.Photo,
                PhotoId = Guid.NewGuid(),
                Photo = new Photo { OriginalFileName = $"photo{i}.jpg" }
            })
            .ToList();

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shareLinks);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(20);
    }

    [Fact]
    public async Task Handle_DifferentUsers_ReturnsOnlyUserShareLinks()
    {
        // Arrange
        var user1Id = "user-1";
        var user2Id = "user-2";
        var baseUrl = "https://example.com";
        var query = new GetShareLinksQuery(user1Id, baseUrl);

        var user1ShareLinks = new List<ShareLink>
        {
            new ShareLink
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                Token = "user1-token",
                Type = ShareLinkType.Photo,
                PhotoId = Guid.NewGuid(),
                Photo = new Photo { OriginalFileName = "user1-photo.jpg" }
            }
        };

        var user2ShareLinks = new List<ShareLink>
        {
            new ShareLink
            {
                Id = Guid.NewGuid(),
                UserId = user2Id,
                Token = "user2-token",
                Type = ShareLinkType.Photo,
                PhotoId = Guid.NewGuid(),
                Photo = new Photo { OriginalFileName = "user2-photo.jpg" }
            }
        };

        _shareLinkRepositoryMock
            .Setup(x => x.GetByUserIdAsync(user1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user1ShareLinks);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Token.Should().Be("user1-token");
        _shareLinkRepositoryMock.Verify(
            x => x.GetByUserIdAsync(user1Id, It.IsAny<CancellationToken>()),
            Times.Once);
        _shareLinkRepositoryMock.Verify(
            x => x.GetByUserIdAsync(user2Id, It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
