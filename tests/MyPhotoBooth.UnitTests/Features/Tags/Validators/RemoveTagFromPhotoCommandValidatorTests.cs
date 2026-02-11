using FluentAssertions;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Features.Tags.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Validators;

public class RemoveTagFromPhotoCommandValidatorTests
{
    private readonly RemoveTagFromPhotoCommandValidator _validator;

    public RemoveTagFromPhotoCommandValidatorTests()
    {
        _validator = new RemoveTagFromPhotoCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_PhotoId_Is_Empty()
    {
        var command = new RemoveTagFromPhotoCommand(Guid.Empty, Guid.NewGuid(), "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoId");
    }

    [Fact]
    public void Should_Have_Error_When_TagId_Is_Empty()
    {
        var command = new RemoveTagFromPhotoCommand(Guid.NewGuid(), Guid.Empty, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TagId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var command = new RemoveTagFromPhotoCommand(Guid.NewGuid(), Guid.NewGuid(), null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new RemoveTagFromPhotoCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new RemoveTagFromPhotoCommand(Guid.NewGuid(), Guid.NewGuid(), "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Guids_Are_Empty()
    {
        var command = new RemoveTagFromPhotoCommand(Guid.Empty, Guid.Empty, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Count().Should().BeGreaterThanOrEqualTo(2);
    }
}
