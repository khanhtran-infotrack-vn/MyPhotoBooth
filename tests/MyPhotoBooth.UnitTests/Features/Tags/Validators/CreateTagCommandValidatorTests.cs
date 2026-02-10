using FluentAssertions;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Features.Tags.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Validators;

public class CreateTagCommandValidatorTests
{
    private readonly CreateTagCommandValidator _validator;

    public CreateTagCommandValidatorTests()
    {
        _validator = new CreateTagCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        var command = new CreateTagCommand(null!, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateTagCommand(string.Empty, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var command = new CreateTagCommand(new string('A', 101), "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Has_Invalid_Characters()
    {
        var command = new CreateTagCommand("Tag@Name!", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Has_Valid_Special_Characters()
    {
        var command = new CreateTagCommand("Tag Name-123_Test", "userId");
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var command = new CreateTagCommand("TagName", null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new CreateTagCommand("TagName", string.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new CreateTagCommand("MyTag", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
