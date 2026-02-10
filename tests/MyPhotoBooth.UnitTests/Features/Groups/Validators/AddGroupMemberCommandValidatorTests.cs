using FluentAssertions;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Features.Groups.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Groups.Validators;

public class AddGroupMemberCommandValidatorTests
{
    private readonly AddGroupMemberCommandValidator _validator;

    public AddGroupMemberCommandValidatorTests()
    {
        _validator = new AddGroupMemberCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_GroupId_Is_Empty()
    {
        var command = new AddGroupMemberCommand(Guid.Empty, "test@example.com", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "GroupId");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        var command = new AddGroupMemberCommand(Guid.NewGuid(), null!, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MemberEmail");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new AddGroupMemberCommand(Guid.NewGuid(), string.Empty, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MemberEmail");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new AddGroupMemberCommand(Guid.NewGuid(), "not-an-email", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "MemberEmail");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new AddGroupMemberCommand(Guid.NewGuid(), "test@example.com", string.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new AddGroupMemberCommand(Guid.NewGuid(), "test@example.com", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
