using FluentAssertions;
using MyPhotoBooth.Application.Features.Auth.Commands;
using MyPhotoBooth.Application.Features.Auth.Validators;
using Xunit;

namespace MyPhotoBooth.UnitTests.Features.Auth.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        var command = new LoginCommand(null!, "TestPassword123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var command = new LoginCommand(string.Empty, "TestPassword123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid()
    {
        var command = new LoginCommand("not-an-email", "TestPassword123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        var command = new LoginCommand("test@example.com", "TestPassword123");
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Null()
    {
        var command = new LoginCommand("test@example.com", null!);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var command = new LoginCommand("test@example.com", string.Empty);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Password_Is_Valid()
    {
        var command = new LoginCommand("test@example.com", "TestPassword123");
        var result = _validator.Validate(command);
        result.Errors.Should().NotContain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var command = new LoginCommand("test@example.com", "TestPassword123");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
