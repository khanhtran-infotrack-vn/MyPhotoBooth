using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Features.Tags.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Handlers;

public class GetTagPhotosQueryHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<IPhotoRepository> _photoRepositoryMock;
    private readonly Mock<ILogger<GetTagPhotosQueryHandler>> _loggerMock;
    private readonly GetTagPhotosQueryHandler _handler;

    public GetTagPhotosQueryHandlerTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _photoRepositoryMock = new Mock<IPhotoRepository>();
        _loggerMock = new Mock<ILogger<GetTagPhotosQueryHandler>>();
        _handler = new GetTagPhotosQueryHandler(_tagRepositoryMock.Object, _photoRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Returns_Paginated_Photos_For_Tag()
    {
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var query = new GetTagPhotosQuery(tagId, userId, 1, 10);

        var tag = new Tag
        {
            Id = tagId,
            Name = "nature",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var photos = new List<Photo>();
        for (int i = 0; i < 15; i++)
        {
            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OriginalFileName = $"photo{i}.jpg",
                FilePath = $"/path/to/photo{i}.jpg",
                ThumbnailPath = $"/path/to/thumb{i}.jpg",
                ContentType = "image/jpeg",
                FileSize = 1000,
                Width = 1920,
                Height = 1080,
                UploadedAt = DateTime.UtcNow.AddMinutes(-i)
            };

            var photoTag = new PhotoTag
            {
                PhotoId = photo.Id,
                TagId = tagId,
                Photo = photo,
                Tag = tag
            };

            tag.PhotoTags.Add(photoTag);
            photos.Add(photo);
        }

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var photoIds = photos.Take(10).Select(p => p.Id).ToList();
        var favoriteStatus = photoIds.ToDictionary(id => id, _ => false);

        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<List<Guid>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(favoriteStatus);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(15);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Returns_Error_When_Tag_Not_Found()
    {
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var query = new GetTagPhotosQuery(tagId, userId);

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Tag not found");
    }

    [Fact]
    public async Task Handle_Returns_Error_When_User_Not_Authorized()
    {
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var query = new GetTagPhotosQuery(tagId, userId);

        var tag = new Tag
        {
            Id = tagId,
            Name = "nature",
            UserId = "different-user-id",
            CreatedAt = DateTime.UtcNow
        };

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
    }

    [Fact]
    public async Task Handle_Returns_Empty_Page_When_Tag_Has_No_Photos()
    {
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var query = new GetTagPhotosQuery(tagId, userId);

        var tag = new Tag
        {
            Id = tagId,
            Name = "nature",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<List<Guid>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Returns_Photos_Ordered_By_UploadedAt_Descending()
    {
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var query = new GetTagPhotosQuery(tagId, userId);

        var tag = new Tag
        {
            Id = tagId,
            Name = "nature",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var photo1 = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OriginalFileName = "photo1.jpg",
            FilePath = "/path/to/photo1.jpg",
            ThumbnailPath = "/path/to/thumb1.jpg",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow.AddHours(-2)
        };

        var photo2 = new Photo
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OriginalFileName = "photo2.jpg",
            FilePath = "/path/to/photo2.jpg",
            ThumbnailPath = "/path/to/thumb2.jpg",
            ContentType = "image/jpeg",
            FileSize = 1000,
            Width = 1920,
            Height = 1080,
            UploadedAt = DateTime.UtcNow.AddHours(-1)
        };

        tag.PhotoTags.Add(new PhotoTag { PhotoId = photo1.Id, TagId = tagId, Photo = photo1, Tag = tag });
        tag.PhotoTags.Add(new PhotoTag { PhotoId = photo2.Id, TagId = tagId, Photo = photo2, Tag = tag });

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<List<Guid>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items[0].Id.Should().Be(photo2.Id);
        result.Value.Items[1].Id.Should().Be(photo1.Id);
    }

    [Fact]
    public async Task Handle_Returns_Second_Page_Correctly()
    {
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var query = new GetTagPhotosQuery(tagId, userId, 2, 10);

        var tag = new Tag
        {
            Id = tagId,
            Name = "nature",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        for (int i = 0; i < 25; i++)
        {
            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OriginalFileName = $"photo{i}.jpg",
                FilePath = $"/path/to/photo{i}.jpg",
                ThumbnailPath = $"/path/to/thumb{i}.jpg",
                ContentType = "image/jpeg",
                FileSize = 1000,
                Width = 1920,
                Height = 1080,
                UploadedAt = DateTime.UtcNow.AddMinutes(-i)
            };

            tag.PhotoTags.Add(new PhotoTag { PhotoId = photo.Id, TagId = tagId, Photo = photo, Tag = tag });
        }

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _photoRepositoryMock
            .Setup(x => x.GetFavoriteStatusAsync(
                It.IsAny<List<Guid>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, bool>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.Page.Should().Be(2);
        result.Value.HasPrevious.Should().BeTrue();
        result.Value.HasNext.Should().BeTrue();
    }
}
