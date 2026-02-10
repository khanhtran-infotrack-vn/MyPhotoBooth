using FluentAssertions;
using MyPhotoBooth.Application.Features.Albums.Commands;
using MyPhotoBooth.Application.Features.Albums.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Albums.Validators;

public class UpdateAlbumCommandValidatorTests
{
    private readonly UpdateAlbumCommandValidator _validator;

    public UpdateAlbumCommandValidatorTests()
    {
        _validator = new UpdateAlbumCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_AlbumId_Is_Empty()
    {
        var command = new UpdateAlbumCommand(Guid.Empty, "Name", "Description", null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AlbumId");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        var command = new UpdateAlbumCommand(Guid.NewGuid(), null!, "Description", null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new UpdateAlbumCommand(Guid.NewGuid(), string.Empty, "Description", null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Too_Long()
    {
        var command = new UpdateAlbumCommand(Guid.NewGuid(), new string('A', 201), "Description", null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Too_Long()
    {
        var command = new UpdateAlbumCommand(Guid.NewGuid(), "Name", new string('A', 1001), null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Description_Is_Null()
    {
        var command = new UpdateAlbumCommand(Guid.NewGuid(), "Name", null, null, "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new UpdateAlbumCommand(Guid.NewGuid(), "Album Name", "Description", Guid.NewGuid(), "userId");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
