using FluentAssertions;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Features.Photos.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class GetFavoritePhotosQueryValidatorTests
{
    private readonly GetFavoritePhotosQueryValidator _validator;

    public GetFavoritePhotosQueryValidatorTests()
    {
        _validator = new GetFavoritePhotosQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var query = new GetFavoritePhotosQuery(null!, 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new GetFavoritePhotosQuery(string.Empty, 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_Page_Is_Zero()
    {
        var query = new GetFavoritePhotosQuery("user-id", 0, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void Should_Have_Error_When_PageSize_Exceeds_Maximum()
    {
        var query = new GetFavoritePhotosQuery("user-id", 1, 101);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var query = new GetFavoritePhotosQuery("user-id", 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }
}
