using FluentAssertions;
using MyPhotoBooth.Application.Features.Tags.Queries;
using MyPhotoBooth.Application.Features.Tags.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Tags.Validators;

public class GetTagPhotosQueryValidatorTests
{
    private readonly GetTagPhotosQueryValidator _validator;

    public GetTagPhotosQueryValidatorTests()
    {
        _validator = new GetTagPhotosQueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_TagId_Is_Empty()
    {
        var query = new GetTagPhotosQuery(Guid.Empty, "userId");
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TagId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), null!);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), string.Empty);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_Page_Is_Zero()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), "userId", 0);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void Should_Have_Error_When_Page_Is_Negative()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), "userId", -1);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Page");
    }

    [Fact]
    public void Should_Have_Error_When_PageSize_Is_Zero()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), "userId", 1, 0);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Should_Have_Error_When_PageSize_Exceeds_Maximum()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), "userId", 1, 101);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PageSize");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), "userId", 1, 50);
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Use_Default_Values_When_Not_Provided()
    {
        var query = new GetTagPhotosQuery(Guid.NewGuid(), "userId");
        var result = _validator.Validate(query);
        result.IsValid.Should().BeTrue();
    }
}
