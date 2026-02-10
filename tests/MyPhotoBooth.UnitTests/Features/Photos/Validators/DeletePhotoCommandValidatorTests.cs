using FluentAssertions;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class DeletePhotoCommandValidatorTests
{
    private readonly DeletePhotoCommandValidator _validator;

    public DeletePhotoCommandValidatorTests()
    {
        _validator = new DeletePhotoCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_PhotoId_Is_Empty()
    {
        var command = new DeletePhotoCommand(Guid.Empty, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhotoId_Is_Valid()
    {
        var command = new DeletePhotoCommand(Guid.NewGuid(), "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
