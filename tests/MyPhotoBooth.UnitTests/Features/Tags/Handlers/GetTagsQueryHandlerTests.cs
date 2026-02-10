using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Features.Tags.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Handlers;

public class GetTagsQueryHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<ILogger<GetTagsQueryHandler>> _loggerMock;
    private readonly GetTagsQueryHandler _handler;

    public GetTagsQueryHandlerTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _loggerMock = new Mock<ILogger<GetTagsQueryHandler>>();
        _handler = new GetTagsQueryHandler(_tagRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_ReturnsTagList()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTagsQuery(userId);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "nature",
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "sunset",
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "beach",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        _tagRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);

        result.Value[0].Name.Should().Be("nature");
        result.Value[1].Name.Should().Be("sunset");
        result.Value[2].Name.Should().Be("beach");

        foreach (var tagResponse in result.Value)
        {
            tagResponse.Id.Should().NotBeEmpty();
            tagResponse.CreatedAt.Should().NotBe(default);
        }
    }

    [Fact]
    public async Task Handle_EmptyTagList_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTagsQuery(userId);

        _tagRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithLargeTagList_ReturnsAllTags()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTagsQuery(userId);

        var tags = Enumerable.Range(1, 50)
            .Select(i => new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = $"tag{i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            })
            .ToList();

        _tagRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(50);
    }

    [Fact]
    public async Task Handle_PreservesTimestamps()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTagsQuery(userId);

        var createdAt = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "test",
                CreatedAt = createdAt
            }
        };

        _tagRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_MultipleUsers_ReturnsOnlyUserTags()
    {
        // Arrange
        var user1Id = "user-1";
        var user2Id = "user-2";
        var query = new GetTagsQuery(user1Id);

        var user1Tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), UserId = user1Id, Name = "nature", CreatedAt = DateTime.UtcNow }
        };

        var user2Tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), UserId = user2Id, Name = "sunset", CreatedAt = DateTime.UtcNow }
        };

        _tagRepositoryMock
            .Setup(x => x.GetByUserIdAsync(user1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user1Tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("nature");
    }

    [Fact]
    public async Task Handle_WithSpecialCharactersInName_PreservesName()
    {
        // Arrange
        var userId = "user-id";
        var query = new GetTagsQuery(userId);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "nature & sunset",
                CreatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "beach/vacation",
                CreatedAt = DateTime.UtcNow
            }
        };

        _tagRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be("nature & sunset");
        result.Value[1].Name.Should().Be("beach/vacation");
    }
}
