using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Auth.Commands;

namespace MyPhotoBooth.Application.Features.Auth.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).Email();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
