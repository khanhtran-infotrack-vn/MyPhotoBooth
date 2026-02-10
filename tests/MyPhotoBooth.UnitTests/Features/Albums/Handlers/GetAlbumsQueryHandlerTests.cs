using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Albums.Queries;
using MyPhotoBooth.Application.Features.Albums.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Albums.Handlers;

public class GetAlbumsQueryHandlerTests
{
    private readonly Mock<IAlbumRepository> _albumRepositoryMock;
    private readonly Mock<ILogger<GetAlbumsQueryHandler>> _loggerMock;
    private readonly GetAlbumsQueryHandler _handler;

    public GetAlbumsQueryHandlerTests()
    {
        _albumRepositoryMock = new Mock<IAlbumRepository>();
        _loggerMock = new Mock<ILogger<GetAlbumsQueryHandler>>();
        _handler = new GetAlbumsQueryHandler(_albumRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsAlbumList()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetAlbumsQuery(userId);

        var albums = new List<Album>
        {
            new Album
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Vacation",
                Description = "Summer trip",
                CoverPhotoId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                AlbumPhotos = new List<AlbumPhoto>
                {
                    new AlbumPhoto(),
                    new AlbumPhoto(),
                    new AlbumPhoto()
                }
            },
            new Album
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Family",
                Description = null,
                CoverPhotoId = null,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                AlbumPhotos = new List<AlbumPhoto>()
            }
        };

        _albumRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(albums);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);

        result.Value[0].Name.Should().Be("Vacation");
        result.Value[0].Description.Should().Be("Summer trip");
        result.Value[0].PhotoCount.Should().Be(3);
        result.Value[0].CoverPhotoId.Should().NotBeNull();

        result.Value[1].Name.Should().Be("Family");
        result.Value[1].Description.Should().BeNull();
        result.Value[1].PhotoCount.Should().Be(0);
        result.Value[1].CoverPhotoId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EmptyAlbumList_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetAlbumsQuery(userId);

        _albumRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Album>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithLargePhotoCount_CalculatesCorrectly()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetAlbumsQuery(userId);

        var album = new Album
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Large Album",
            AlbumPhotos = Enumerable.Range(0, 150)
                .Select(_ => new AlbumPhoto())
                .ToList()
        };

        _albumRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Album> { album });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].PhotoCount.Should().Be(150);
    }

    [Fact]
    public async Task Handle_PreservesTimestamps()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetAlbumsQuery(userId);

        var createdAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var updatedAt = new DateTime(2024, 1, 15, 12, 30, 0, DateTimeKind.Utc);

        var album = new Album
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Test Album",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            AlbumPhotos = new List<AlbumPhoto>()
        };

        _albumRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Album> { album });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].CreatedAt.Should().Be(createdAt);
        result.Value[0].UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task Handle_MultipleAlbums_ReturnsAllAlbumsForUser()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetAlbumsQuery(userId);

        var albums = Enumerable.Range(1, 10)
            .Select(i => new Album
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = $"Album {i}",
                AlbumPhotos = new List<AlbumPhoto>()
            })
            .ToList();

        _albumRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(albums);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);

        for (int i = 0; i < 10; i++)
        {
            result.Value[i].Name.Should().Be($"Album {i + 1}");
        }
    }
}
