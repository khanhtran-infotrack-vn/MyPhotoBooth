using FluentAssertions;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Features.Photos.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class SearchPhotosQueryValidatorTests
{
    private readonly SearchPhotosQueryValidator _validator;

    public SearchPhotosQueryValidatorTests()
    {
        _validator = new SearchPhotosQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_SearchTerm_Is_Null()
    {
        var query = new SearchPhotosQuery(null!, "user-id", 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SearchTerm");
    }

    [Fact]
    public void Should_Have_Error_When_SearchTerm_Is_Empty()
    {
        var query = new SearchPhotosQuery(string.Empty, "user-id", 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SearchTerm");
    }

    [Fact]
    public void Should_Have_Error_When_SearchTerm_Is_Too_Short()
    {
        var query = new SearchPhotosQuery("a", "user-id", 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SearchTerm");
    }

    [Fact]
    public void Should_Have_Error_When_SearchTerm_Is_Too_Long()
    {
        var query = new SearchPhotosQuery(new string('a', 101), "user-id", 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SearchTerm");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var query = new SearchPhotosQuery("test", null!, 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_Page_Is_Zero()
    {
        var query = new SearchPhotosQuery("test", "user-id", 0, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void Should_Have_Error_When_PageSize_Exceeds_Maximum()
    {
        var query = new SearchPhotosQuery("test", "user-id", 1, 101);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var query = new SearchPhotosQuery("test search", "user-id", 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }
}
