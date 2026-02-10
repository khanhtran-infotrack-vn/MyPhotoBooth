using FluentValidation;
using MyPhotoBooth.Application.Features.Groups.Commands;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class AddGroupMemberCommandValidator : AbstractValidator<AddGroupMemberCommand>
{
    public AddGroupMemberCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage(Errors.Groups.NotFound);
        RuleFor(x => x.MemberEmail).NotEmpty().EmailAddress().WithMessage(Errors.Groups.InvalidEmail);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(Errors.Auth.UserNotFound);
    }
}
