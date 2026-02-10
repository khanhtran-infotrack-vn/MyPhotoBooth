using FluentValidation;
using MyPhotoBooth.Application.Features.Groups.Commands;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class RemoveGroupMemberCommandValidator : AbstractValidator<RemoveGroupMemberCommand>
{
    public RemoveGroupMemberCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage(Errors.Groups.NotFound);
        RuleFor(x => x.MemberUserId).NotEmpty().WithMessage(Errors.Groups.UserNotFound);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(Errors.Auth.UserNotFound);
    }
}
