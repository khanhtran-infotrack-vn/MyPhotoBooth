using FluentAssertions;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class ToggleFavoritePhotoCommandValidatorTests
{
    private readonly ToggleFavoritePhotoCommandValidator _validator;

    public ToggleFavoritePhotoCommandValidatorTests()
    {
        _validator = new ToggleFavoritePhotoCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_PhotoId_Is_Empty()
    {
        var command = new ToggleFavoritePhotoCommand(Guid.Empty, "user-id");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var command = new ToggleFavoritePhotoCommand(Guid.NewGuid(), null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new ToggleFavoritePhotoCommand(Guid.NewGuid(), string.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new ToggleFavoritePhotoCommand(Guid.NewGuid(), "user-id");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Whitespace()
    {
        var command = new ToggleFavoritePhotoCommand(Guid.NewGuid(), "   ");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }
}
