using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Tags.Commands;

namespace MyPhotoBooth.Application.Features.Tags.Validators;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name).TagName();

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
