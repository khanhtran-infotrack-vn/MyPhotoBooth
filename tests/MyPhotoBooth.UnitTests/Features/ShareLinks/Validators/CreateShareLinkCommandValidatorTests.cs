using FluentAssertions;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Application.Features.ShareLinks.Validators;
using MyPhotoBooth.Domain.Entities;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.ShareLinks.Validators;

public class CreateShareLinkCommandValidatorTests
{
    private readonly CreateShareLinkCommandValidator _validator;

    public CreateShareLinkCommandValidatorTests()
    {
        _validator = new CreateShareLinkCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Type_Is_Invalid()
    {
        var command = new CreateShareLinkCommand(
            (ShareLinkType)999,
            Guid.NewGuid(),
            null,
            null,
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Type");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Type_Is_Valid_Photo()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            Guid.NewGuid(),
            null,
            null,
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "Type");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Type_Is_Valid_Album()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Album,
            null,
            Guid.NewGuid(),
            null,
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "Type");
    }

    [Fact]
    public void Should_Have_Error_When_PhotoId_Is_Empty_For_Photo_Type()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            null,
            null,
            null,
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PhotoId");
    }

    [Fact]
    public void Should_Have_Error_When_AlbumId_Is_Empty_For_Album_Type()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Album,
            null,
            null,
            null,
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AlbumId");
    }

    [Fact]
    public void Should_Have_Error_When_ExpiresAt_Is_In_Past()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            Guid.NewGuid(),
            null,
            DateTime.UtcNow.AddDays(-1),
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ExpiresAt");
    }

    [Fact]
    public void Should_Not_Have_Error_When_ExpiresAt_Is_In_Future()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            Guid.NewGuid(),
            null,
            DateTime.UtcNow.AddDays(7),
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "ExpiresAt");
    }

    [Fact]
    public void Should_Not_Have_Error_When_ExpiresAt_Is_Null()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            Guid.NewGuid(),
            null,
            null,
            true,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "ExpiresAt");
    }

    [Fact]
    public void Should_Have_Error_When_UserId_Is_Null()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            Guid.NewGuid(),
            null,
            null,
            true,
            null,
            null!,
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Should_Have_Error_When_BaseUrl_Is_Null()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            Guid.NewGuid(),
            null,
            null,
            true,
            null,
            "userId",
            null!
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BaseUrl");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid_For_Photo()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Photo,
            Guid.NewGuid(),
            null,
            DateTime.UtcNow.AddDays(7),
            true,
            "Password123",
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid_For_Album()
    {
        var command = new CreateShareLinkCommand(
            ShareLinkType.Album,
            null,
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30),
            false,
            null,
            "userId",
            "http://localhost"
        );
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
