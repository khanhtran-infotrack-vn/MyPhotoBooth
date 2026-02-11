using FluentAssertions;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Features.Tags.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Validators;

public class GetTagsWithPhotoCountQueryValidatorTests
{
    private readonly GetTagsWithPhotoCountQueryValidator _validator;

    public GetTagsWithPhotoCountQueryValidatorTests()
    {
        _validator = new GetTagsWithPhotoCountQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var query = new GetTagsWithPhotoCountQuery(null!);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new GetTagsWithPhotoCountQuery(string.Empty);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        var query = new GetTagsWithPhotoCountQuery("userId");
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }
}
