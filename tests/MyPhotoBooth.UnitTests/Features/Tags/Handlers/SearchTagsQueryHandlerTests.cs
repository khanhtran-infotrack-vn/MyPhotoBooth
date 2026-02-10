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

public class SearchTagsQueryHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<ILogger<SearchTagsQueryHandler>> _loggerMock;
    private readonly SearchTagsQueryHandler _handler;

    public SearchTagsQueryHandlerTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _loggerMock = new Mock<ILogger<SearchTagsQueryHandler>>();
        _handler = new SearchTagsQueryHandler(_tagRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidSearch_ReturnsMatchingTags()
    {
        // Arrange
        var userId = "user-id";
        var searchQuery = "nat";
        var query = new SearchTagsQuery(searchQuery, userId);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "nature",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "native",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            }
        };

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("nature");
        result.Value[1].Name.Should().Be("native");

        _tagRepositoryMock.Verify(
            x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoMatchingResults_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user-id";
        var searchQuery = "xyz";
        var query = new SearchTagsQuery(searchQuery, userId);

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptySearchQuery_ReturnsAllTags()
    {
        // Arrange
        var userId = "user-id";
        var searchQuery = "";
        var query = new SearchTagsQuery(searchQuery, userId);

        var tags = new List<Tag>
        {
            new Tag { Id = Guid.NewGuid(), UserId = userId, Name = "nature", CreatedAt = DateTime.UtcNow },
            new Tag { Id = Guid.NewGuid(), UserId = userId, Name = "sunset", CreatedAt = DateTime.UtcNow }
        };

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_CaseInsensitiveSearch_SearchesCorrectly()
    {
        // Arrange
        var userId = "user-id";
        var searchQuery = "NATURE";
        var query = new SearchTagsQuery(searchQuery, userId);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "nature",
                CreatedAt = DateTime.UtcNow
            }
        };

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tagRepositoryMock.Verify(
            x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithSpecialCharacters_SearchesCorrectly()
    {
        // Arrange
        var userId = "user-id";
        var searchQuery = "nature &";
        var query = new SearchTagsQuery(searchQuery, userId);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "nature & sunset",
                CreatedAt = DateTime.UtcNow
            }
        };

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be("nature & sunset");
    }

    [Fact]
    public async Task Handle_PartialWordMatch_ReturnsResults()
    {
        // Arrange
        var userId = "user-id";
        var searchQuery = "nat";
        var query = new SearchTagsQuery(searchQuery, userId);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "nature",
                CreatedAt = DateTime.UtcNow
            }
        };

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNumbersInSearch_ReturnsResults()
    {
        // Arrange
        var userId = "user-id";
        var searchQuery = "2024";
        var query = new SearchTagsQuery(searchQuery, userId);

        var tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "vacation 2024",
                CreatedAt = DateTime.UtcNow
            }
        };

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].Name.Should().Be("vacation 2024");
    }

    [Fact]
    public async Task Handle_DifferentUsers_ReturnsOnlyUserTags()
    {
        // Arrange
        var user1Id = "user-1";
        var user2Id = "user-2";
        var searchQuery = "nature";
        var query = new SearchTagsQuery(searchQuery, user1Id);

        var user1Tags = new List<Tag>
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                UserId = user1Id,
                Name = "nature",
                CreatedAt = DateTime.UtcNow
            }
        };

        _tagRepositoryMock
            .Setup(x => x.SearchAsync(searchQuery, user1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user1Tags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        _tagRepositoryMock.Verify(
            x => x.SearchAsync(searchQuery, user1Id, It.IsAny<CancellationToken>()),
            Times.Once);
        _tagRepositoryMock.Verify(
            x => x.SearchAsync(It.IsAny<string>(), user2Id, It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
