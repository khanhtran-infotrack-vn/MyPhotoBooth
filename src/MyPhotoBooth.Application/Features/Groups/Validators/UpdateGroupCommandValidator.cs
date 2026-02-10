using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Groups.Commands;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("Group ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Group name is required")
            .MaximumLength(200).WithMessage("Group name too long");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description too long");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
