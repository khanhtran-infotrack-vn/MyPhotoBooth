using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Features.Tags.Handlers;
using MyPhotoBooth.Application.Interfaces;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Handlers;

public class GetTagsWithPhotoCountQueryHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<ILogger<GetTagsWithPhotoCountQueryHandler>> _loggerMock;
    private readonly GetTagsWithPhotoCountQueryHandler _handler;

    public GetTagsWithPhotoCountQueryHandlerTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _loggerMock = new Mock<ILogger<GetTagsWithPhotoCountQueryHandler>>();
        _handler = new GetTagsWithPhotoCountQueryHandler(_tagRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_Returns_Tags_With_Photo_Counts()
    {
        var userId = "user-id";
        var query = new GetTagsWithPhotoCountQuery(userId);

        var tagsWithCount = new List<TagWithPhotoCountResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "nature", CreatedAt = DateTime.UtcNow, PhotoCount = 5 },
            new() { Id = Guid.NewGuid(), Name = "travel", CreatedAt = DateTime.UtcNow, PhotoCount = 3 },
            new() { Id = Guid.NewGuid(), Name = "sunset", CreatedAt = DateTime.UtcNow, PhotoCount = 0 }
        };

        _tagRepositoryMock
            .Setup(x => x.GetTagsWithPhotoCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagsWithCount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].Name.Should().Be("nature");
        result.Value[0].PhotoCount.Should().Be(5);
        result.Value[1].Name.Should().Be("travel");
        result.Value[1].PhotoCount.Should().Be(3);
        result.Value[2].Name.Should().Be("sunset");
        result.Value[2].PhotoCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Returns_Empty_List_When_User_Has_No_Tags()
    {
        var userId = "user-id";
        var query = new GetTagsWithPhotoCountQuery(userId);

        _tagRepositoryMock
            .Setup(x => x.GetTagsWithPhotoCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TagWithPhotoCountResponse>());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_Returns_Tags_Ordered_By_Name()
    {
        var userId = "user-id";
        var query = new GetTagsWithPhotoCountQuery(userId);

        var tagsWithCount = new List<TagWithPhotoCountResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "zebra", CreatedAt = DateTime.UtcNow, PhotoCount = 1 },
            new() { Id = Guid.NewGuid(), Name = "apple", CreatedAt = DateTime.UtcNow, PhotoCount = 2 },
            new() { Id = Guid.NewGuid(), Name = "banana", CreatedAt = DateTime.UtcNow, PhotoCount = 3 }
        };

        _tagRepositoryMock
            .Setup(x => x.GetTagsWithPhotoCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tagsWithCount);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be("zebra");
        result.Value[1].Name.Should().Be("apple");
        result.Value[2].Name.Should().Be("banana");
    }
}
