using FluentValidation;
using MyPhotoBooth.Application.Features.Groups.Commands;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class SharePhotoToGroupCommandValidator : AbstractValidator<SharePhotoToGroupCommand>
{
    public SharePhotoToGroupCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage(Errors.Groups.NotFound);
        RuleFor(x => x.PhotoId).NotEmpty().WithMessage(Errors.Groups.PhotoNotFound);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(Errors.Auth.UserNotFound);
    }
}
