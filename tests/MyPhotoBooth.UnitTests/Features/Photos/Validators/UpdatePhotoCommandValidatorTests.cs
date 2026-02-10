using FluentAssertions;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class UpdatePhotoCommandValidatorTests
{
    private readonly UpdatePhotoCommandValidator _validator;

    public UpdatePhotoCommandValidatorTests()
    {
        _validator = new UpdatePhotoCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_PhotoId_Is_Empty()
    {
        var command = new UpdatePhotoCommand(Guid.Empty, "description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhotoId_Is_Valid()
    {
        var command = new UpdatePhotoCommand(Guid.NewGuid(), "description", "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Have_Error_When_Description_Too_Long()
    {
        var command = new UpdatePhotoCommand(Guid.NewGuid(), new string('A', 1001), "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Null()
    {
        var command = new UpdatePhotoCommand(Guid.NewGuid(), null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Empty()
    {
        var command = new UpdatePhotoCommand(Guid.NewGuid(), string.Empty, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
