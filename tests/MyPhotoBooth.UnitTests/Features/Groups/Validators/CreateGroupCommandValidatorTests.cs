using FluentAssertions;
using MyPhotoBooth.Application.Features.Groups.Commands;
using MyPhotoBooth.Application.Features.Groups.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Groups.Validators;

public class CreateGroupCommandValidatorTests
{
    private readonly CreateGroupCommandValidator _validator;

    public CreateGroupCommandValidatorTests()
    {
        _validator = new CreateGroupCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        var command = new CreateGroupCommand(null!, "Description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateGroupCommand(string.Empty, "Description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var command = new CreateGroupCommand(new string('A', 201), "Description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Null()
    {
        var command = new CreateGroupCommand("My Group", null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var command = new CreateGroupCommand("My Group", "Description", null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new CreateGroupCommand("My Group", "A description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
