using FluentAssertions;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Features.Photos.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class GetPhotosQueryValidatorTests
{
    private readonly GetPhotosQueryValidator _validator;

    public GetPhotosQueryValidatorTests()
    {
        _validator = new GetPhotosQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Page_Is_Zero()
    {
        var query = new GetPhotosQuery(Page: 0);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void Should_Have_Error_When_Page_Is_Negative()
    {
        var query = new GetPhotosQuery(Page: -1);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void Should_Have_Error_When_PageSize_Is_Zero()
    {
        var query = new GetPhotosQuery(PageSize: 0);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Should_Have_Error_When_PageSize_Is_Too_Large()
    {
        var query = new GetPhotosQuery(PageSize: 201);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Should_Not_Have_Error_With_Default_Values()
    {
        var query = new GetPhotosQuery();
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_With_Valid_Values()
    {
        var query = new GetPhotosQuery(Page: 2, PageSize: 25);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_AlbumId_Is_Null()
    {
        var query = new GetPhotosQuery(AlbumId: null);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Search_Is_Null()
    {
        var query = new GetPhotosQuery(Search: null);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }
}
