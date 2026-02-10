using FluentAssertions;
using MyPhotoBooth.Application.Features.Auth.Commands;
using MyPhotoBooth.Application.Features.Auth.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Auth.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        var command = new RegisterCommand(null!, "TestPassword123", "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new RegisterCommand(string.Empty, "TestPassword123", "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new RegisterCommand("not-an-email", "TestPassword123", "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword123", "Test User");
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Null()
    {
        var command = new RegisterCommand("test@example.com", null!, "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new RegisterCommand("test@example.com", string.Empty, "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var command = new RegisterCommand("test@example.com", "Test1!", "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Uppercase()
    {
        var command = new RegisterCommand("test@example.com", "testpassword123", "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Missing_Digit()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword", "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Password_Is_Valid()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword123", "Test User");
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Have_Error_When_DisplayName_Is_Null()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword123", null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Should_Have_Error_When_DisplayName_Is_Empty()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword123", string.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Should_Have_Error_When_DisplayName_Is_Too_Short()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword123", "A");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Should_Have_Error_When_DisplayName_Is_Too_Long()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword123", new string('A', 101));
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DisplayName");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new RegisterCommand("test@example.com", "TestPassword123", "Test User");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
