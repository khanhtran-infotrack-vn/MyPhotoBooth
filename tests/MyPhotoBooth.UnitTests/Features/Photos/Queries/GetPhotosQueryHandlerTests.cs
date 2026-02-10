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
using MyPhotoBooth.UnitTests.Helpers;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Queries;

public class GetPhotosQueryHandlerTests
{
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<GetPhotosQueryHandler>> _loggerMock;
    private readonly GetPhotosQueryHandler _handler;

    public GetPhotosQueryHandlerTests()
    {
        _photoRepositoryMock = TestHelpers.CreatePhotoRepositoryMock();
        _loggerMock = new Mock<ILogger<GetPhotosQueryHandler>>();
        _handler = new GetPhotosQueryHandler(_photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsPaginatedPhotos()
    {
        // Arrange
        var userId = "test-user-id";
        var photos = new List<Photo>
        {
            new Photo
            {
                Id = Guid.NewGuid(),
                OriginalFileName = "test1.jpg",
                Width = 1920,
                Height = 1080,
                UploadedAt = DateTime.UtcNow,
                ThumbnailPath = "/thumbs/test1.jpg"
            },
            new Photo
            {
                Id = Guid.NewGuid(),
                OriginalFileName = "test2.jpg",
                Width = 1280,
                Height = 720,
                UploadedAt = DateTime.UtcNow,
                ThumbnailPath = "/thumbs/test2.jpg"
            }
        };

        _photoRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(r => r.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var query = new GetPhotosQuery(1, 50, null, null, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(50);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);

        _photoRepositoryMock.Verify(
            r => r.GetByUserIdAsync(userId, 0, 50, It.IsAny<CancellationToken>()),
            Times.Once);
        _photoRepositoryMock.Verify(
            r => r.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyList_ReturnsEmptyPaginatedResult()
    {
        // Arrange
        var userId = "test-user-id";
        var photos = new List<Photo>();

        _photoRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 0, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(r => r.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var query = new GetPhotosQuery(1, 50, null, null, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_CalculatesCorrectSkip()
    {
        // Arrange
        var userId = "test-user-id";
        var photos = new List<Photo>();
        var totalCount = 100;

        _photoRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId, 10, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photos);

        _photoRepositoryMock
            .Setup(r => r.GetCountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);

        var query = new GetPhotosQuery(2, 25, null, null, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(25);
        result.Value.TotalPages.Should().Be(4); // 100 / 25 = 4

        _photoRepositoryMock.Verify(
            r => r.GetByUserIdAsync(userId, 25, 25, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
