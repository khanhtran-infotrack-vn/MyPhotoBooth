using FluentAssertions;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Features.Albums.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Albums.Validators;

public class CreateAlbumCommandValidatorTests
{
    private readonly CreateAlbumCommandValidator _validator;

    public CreateAlbumCommandValidatorTests()
    {
        _validator = new CreateAlbumCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        var command = new CreateAlbumCommand(null!, "description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateAlbumCommand(string.Empty, "description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var command = new CreateAlbumCommand(new string('A', 201), "description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Too_Long()
    {
        var command = new CreateAlbumCommand("Album", new string('A', 1001), "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Null()
    {
        var command = new CreateAlbumCommand("Album", null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var command = new CreateAlbumCommand("Album", "description", null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new CreateAlbumCommand("Album", "description", string.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new CreateAlbumCommand("My Album", "A description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
