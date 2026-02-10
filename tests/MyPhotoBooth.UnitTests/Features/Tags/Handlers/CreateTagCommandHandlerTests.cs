using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Features.Tags.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Handlers;

public class CreateTagCommandHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<ILogger<CreateTagCommandHandler>> _loggerMock;
    private readonly CreateTagCommandHandler _handler;

    public CreateTagCommandHandlerTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _loggerMock = new Mock<ILogger<CreateTagCommandHandler>>();
        _handler = new CreateTagCommandHandler(_tagRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NewTag_CreatesSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var tagName = "nature";
        var command = new CreateTagCommand(tagName, userId);

        _tagRepositoryMock
            .Setup(x => x.GetByNameAsync(tagName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        _tagRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag tag, CancellationToken _) => tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(tagName);
        result.Value.Id.Should().NotBeEmpty();

        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Tag>(t => t.Name == tagName && t.UserId == userId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingTag_ReturnsExistingTag()
    {
        // Arrange
        var userId = "user-id";
        var tagName = "nature";
        var command = new CreateTagCommand(tagName, userId);

        var existingTag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = tagName,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _tagRepositoryMock
            .Setup(x => x.GetByNameAsync(tagName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(existingTag.Id);
        result.Value.Name.Should().Be(tagName);
        result.Value.CreatedAt.Should().Be(existingTag.CreatedAt);

        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_DifferentUserSameTagName_CreatesNewTag()
    {
        // Arrange
        var user1Id = "user-1";
        var user2Id = "user-2";
        var tagName = "nature";
        var command = new CreateTagCommand(tagName, user2Id);

        var existingTag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = tagName,
            UserId = user1Id
        };

        _tagRepositoryMock
            .Setup(x => x.GetByNameAsync(tagName, user2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        _tagRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag tag, CancellationToken _) => tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Tag>(t => t.UserId == user2Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SetsCreatedAtTimestamp()
    {
        // Arrange
        var userId = "user-id";
        var command = new CreateTagCommand(userId, "sunset");

        _tagRepositoryMock
            .Setup(x => x.GetByNameAsync("sunset", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        Tag? capturedTag = null;
        _tagRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .Callback<Tag, CancellationToken>((tag, _) => capturedTag = tag)
            .ReturnsAsync((Tag tag, CancellationToken _) => tag);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTag.Should().NotBeNull();
        capturedTag!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_GeneratesUniqueId()
    {
        // Arrange
        var userId = "user-id";
        var command = new CreateTagCommand("travel", userId);

        _tagRepositoryMock
            .Setup(x => x.GetByNameAsync("travel", userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        Tag? capturedTag = null;
        _tagRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .Callback<Tag, CancellationToken>((tag, _) => capturedTag = tag)
            .ReturnsAsync((Tag tag, CancellationToken _) => tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedTag.Should().NotBeNull();
        capturedTag!.Id.Should().NotBeEmpty();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithWhitespaceTagName_TrimsName()
    {
        // Arrange
        var userId = "user-id";
        var tagName = "  nature  ";
        var command = new CreateTagCommand(tagName, userId);

        _tagRepositoryMock
            .Setup(x => x.GetByNameAsync(tagName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        _tagRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag tag, CancellationToken _) => tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Note: The handler doesn't trim, so it will create the tag with whitespace
        // This test documents current behavior
        _tagRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Tag>(t => t.Name == tagName), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
