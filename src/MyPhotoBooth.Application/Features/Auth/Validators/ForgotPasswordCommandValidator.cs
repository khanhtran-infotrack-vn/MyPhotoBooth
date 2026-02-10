using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Auth.Commands;

namespace MyPhotoBooth.Application.Features.Auth.Validators;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email).Email();

        RuleFor(x => x.ResetUrlTemplate)
            .NotEmpty().WithMessage("Reset URL template is required")
            .Must(x => x.Contains("{token}")).WithMessage("Reset URL must contain {token} placeholder")
            .Must(x => x.Contains("{email}")).WithMessage("Reset URL must contain {email} placeholder");
    }
}
