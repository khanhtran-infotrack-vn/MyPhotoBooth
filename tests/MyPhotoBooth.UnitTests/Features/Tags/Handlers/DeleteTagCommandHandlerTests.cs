using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Features.Tags.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Handlers;

public class DeleteTagCommandHandlerTests
{
    private readonly Mock<ITagRepository> _tagRepositoryMock;
    private readonly Mock<ILogger<DeleteTagCommandHandler>> _loggerMock;
    private readonly DeleteTagCommandHandler _handler;

    public DeleteTagCommandHandlerTests()
    {
        _tagRepositoryMock = new Mock<ITagRepository>();
        _loggerMock = new Mock<ILogger<DeleteTagCommandHandler>>();
        _handler = new DeleteTagCommandHandler(_tagRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOwner_DeletesTagSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId, userId);

        var tag = new Tag
        {
            Id = tagId,
            UserId = userId,
            Name = "nature"
        };

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _tagRepositoryMock
            .Setup(x => x.DeleteAsync(tagId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _tagRepositoryMock.Verify(x => x.DeleteAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentTag_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId, userId);

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Tag not found");
        _tagRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ReturnsFailure()
    {
        // Arrange
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId, userId);

        var tag = new Tag
        {
            Id = tagId,
            UserId = "different-user-id",
            Name = "nature"
        };

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("You are not authorized to perform this action");
        _tagRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryException_PropagatesException()
    {
        // Arrange
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId, userId);

        var tag = new Tag { Id = tagId, UserId = userId };

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _tagRepositoryMock
            .Setup(x => x.DeleteAsync(tagId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TagWithPhotos_DeletesTagSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var tagId = Guid.NewGuid();
        var command = new DeleteTagCommand(tagId, userId);

        var tag = new Tag
        {
            Id = tagId,
            UserId = userId,
            Name = "nature",
            PhotoTags = new List<PhotoTag>
            {
                new PhotoTag { PhotoId = Guid.NewGuid() },
                new PhotoTag { PhotoId = Guid.NewGuid() }
            }
        };

        _tagRepositoryMock
            .Setup(x => x.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _tagRepositoryMock
            .Setup(x => x.DeleteAsync(tagId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // The repository should handle cascade deletion
        _tagRepositoryMock.Verify(x => x.DeleteAsync(tagId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
