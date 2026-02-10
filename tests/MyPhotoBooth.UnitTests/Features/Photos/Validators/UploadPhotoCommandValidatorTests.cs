using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Features.Photos.Validators;
using MyPhotoBooth.UnitTests.Helpers;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Photos.Validators;

public class UploadPhotoCommandValidatorTests
{
    private readonly UploadPhotoCommandValidator _validator;

    public UploadPhotoCommandValidatorTests()
    {
        _validator = new UploadPhotoCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Null()
    {
        // Arrange
        var command = new UploadPhotoCommand(null!, "description", "userId");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "File");
    }

    [Fact]
    public void Should_Not_Have_Error_When_File_Is_Valid()
    {
        // Arrange
        var file = TestHelpers.CreateTestFormFile();
        var command = new UploadPhotoCommand(file, "description", "userId");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Empty()
    {
        // Arrange
        var file = TestHelpers.CreateTestFormFile(size: 0);
        var command = new UploadPhotoCommand(file, "description", "userId");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "File");
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Too_Large()
    {
        // Arrange
        var largeFileSize = 51 * 1024 * 1024; // 51MB
        var file = TestHelpers.CreateTestFormFile(size: largeFileSize);
        var command = new UploadPhotoCommand(file, "description", "userId");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "File" && e.ErrorMessage.Contains("exceeds"));
    }

    [Fact]
    public void Should_Have_Error_When_File_Is_Not_Image()
    {
        // Arrange
        var file = TestHelpers.CreateTestFormFile(contentType: "application/pdf");
        var command = new UploadPhotoCommand(file, "description", "userId");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "File" && e.ErrorMessage.Contains("image"));
    }

    [Fact]
    public void Should_Have_Error_When_Description_Too_Long()
    {
        // Arrange
        var file = TestHelpers.CreateTestFormFile();
        var longDescription = new string('A', 1001);
        var command = new UploadPhotoCommand(file, longDescription, "userId");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
