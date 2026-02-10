using FluentValidation;
using MyPhotoBooth.Application.Features.Groups.Commands;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class LeaveGroupCommandValidator : AbstractValidator<LeaveGroupCommand>
{
    public LeaveGroupCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage(Errors.Groups.NotFound);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(Errors.Auth.UserNotFound);
    }
}
