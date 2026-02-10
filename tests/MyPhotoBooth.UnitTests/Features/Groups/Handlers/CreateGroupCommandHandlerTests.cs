using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Features.Groups.Handlers;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Groups.Handlers;

public class CreateGroupCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepositoryMock;
    private readonly Mock<ILogger<CreateGroupCommandHandler>> _loggerMock;
    private readonly CreateGroupCommandHandler _handler;

    public CreateGroupCommandHandlerTests()
    {
        _groupRepositoryMock = new Mock<IGroupRepository>();
        _loggerMock = new Mock<ILogger<CreateGroupCommandHandler>>();
        _handler = new CreateGroupCommandHandler(_groupRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesGroupSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var name = "My Group";
        var description = "A test group";
        var command = new CreateGroupCommand(name, description, userId);

        Group? capturedGroup = null;
        _groupRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .Callback<Group, CancellationToken>((group, _) => capturedGroup = group)
            .ReturnsAsync((Group group, CancellationToken _) => group);

        _groupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember member, CancellationToken _) => member);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.OwnerId.Should().Be(userId);
        result.Value.MemberCount.Should().Be(1);
        result.Value.IsOwner.Should().BeTrue();

        _groupRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Group>(g =>
                g.Name == name &&
                g.Description == description &&
                g.OwnerId == userId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullDescription_CreatesGroupSuccessfully()
    {
        // Arrange
        var userId = "user-id";
        var name = "My Group";
        var command = new CreateGroupCommand(name, null, userId);

        _groupRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Group group, CancellationToken _) => group);

        _groupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember member, CancellationToken _) => member);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GeneratesUniqueId()
    {
        // Arrange
        var userId = "user-id";
        var command = new CreateGroupCommand("Test Group", null, userId);

        Group? capturedGroup = null;
        _groupRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Group>(), It.IsAny<CancellationToken>()))
            .Callback<Group, CancellationToken>((group, _) => capturedGroup = group)
            .ReturnsAsync((Group group, CancellationToken _) => group);

        _groupRepositoryMock
            .Setup(x => x.AddMemberAsync(It.IsAny<GroupMember>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GroupMember member, CancellationToken _) => member);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedGroup.Should().NotBeNull();
        capturedGroup!.Id.Should().NotBeEmpty();
        result.Value.Id.Should().NotBeEmpty();
    }
}
