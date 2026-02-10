using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Features.Photos.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Handlers;

public class GetTimelineQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<GetTimelineQueryHandler>> _loggerMock;
    private readonly GetTimelineQueryHandler _handler;

    public GetTimelineQueryHandlerTests()
    {
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        // Setup default behavior for GetFavoriteStatusAsync
        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<IEnumerable<Guid>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());
        _loggerMock = new Mock<ILogger<GetTimelineQueryHandler>>();
        _handler = new GetTimelineQueryHandler(_photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsTimelinePhotos()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTimelineQuery(null, null, 1, 25, userId);

        var photos = new List<Photo>
        {
            new Photo
            {
                Id = Guid.NewGuid(),
                OriginalFileName = "photo1.jpg",
                Width = 1920,
                Height = 1080,
                CapturedAt = new DateTime(2024, 1, 15),
                UploadedAt = DateTime.UtcNow,
                ThumbnailPath = "/thumbs/photo1.jpg"
            },
            new Photo
            {
                Id = Guid.NewGuid(),
                OriginalFileName = "photo2.jpg",
                Width = 1280,
                Height = 720,
                CapturedAt = new DateTime(2024, 1, 16),
                UploadedAt = DateTime.UtcNow,
                ThumbnailPath = "/thumbs/photo2.jpg"
            }
        };

        _photoRepositoryMock
            .Setup(x => x.GetTimelineAsync(userId, null, null, 0, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(x => x.GetTimelineCountAsync(userId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(25);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithYearMonthFilter_FiltersByDateRange()
    {
        // Arrange
        var userId = "user-id";
        var year = 2024;
        var month = 1;
        var query = new GetTimelineQuery(year, month, 1, 25, userId);

        var photos = new List<Photo>();
        var expectedFromDate = new DateTime(year, month, 1);
        var expectedToDate = expectedFromDate.AddMonths(1);

        _photoRepositoryMock
            .Setup(x => x.GetTimelineAsync(userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 0, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(x => x.GetTimelineCountAsync(userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _photoRepositoryMock.Verify(
            x => x.GetTimelineAsync(userId, expectedFromDate, expectedToDate, 0, 25, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithYearOnlyFilter_FiltersByYear()
    {
        // Arrange
        var userId = "user-id";
        var year = 2024;
        var query = new GetTimelineQuery(year, null, 1, 25, userId);

        var photos = new List<Photo>();
        var expectedFromDate = new DateTime(year, 1, 1);
        var expectedToDate = expectedFromDate.AddYears(1);

        _photoRepositoryMock
            .Setup(x => x.GetTimelineAsync(userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), 0, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(x => x.GetTimelineCountAsync(userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _photoRepositoryMock.Verify(
            x => x.GetTimelineAsync(userId, expectedFromDate, expectedToDate, 0, 25, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_CalculatesCorrectSkip()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTimelineQuery(null, null, 2, 20, userId);

        var photos = new List<Photo>();

        _photoRepositoryMock
            .Setup(x => x.GetTimelineAsync(userId, null, null, 20, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(x => x.GetTimelineCountAsync(userId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.TotalPages.Should().Be(3); // 50 / 20 = 2.5 -> 3
    }

    [Fact]
    public async Task Handle_EmptyTimeline_ReturnsEmptyPaginatedResult()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTimelineQuery(null, null, 1, 25, userId);

        _photoRepositoryMock
            .Setup(x => x.GetTimelineAsync(userId, null, null, 0, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Photo>());

        _photoRepositoryMock
            .Setup(x => x.GetTimelineCountAsync(userId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NullUserId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        string? nullUserId = null;
        var query = new GetTimelineQuery(null, null, 1, 25, nullUserId!);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.Handle(query, CancellationToken.None));
    }
}
