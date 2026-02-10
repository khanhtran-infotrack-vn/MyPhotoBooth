using FluentValidation;
using MyPhotoBooth.Application.Features.Groups.Commands;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class RemoveAlbumFromGroupCommandValidator : AbstractValidator<RemoveAlbumFromGroupCommand>
{
    public RemoveAlbumFromGroupCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty().WithMessage(Errors.Groups.NotFound);
        RuleFor(x => x.AlbumId).NotEmpty().WithMessage(Errors.Groups.AlbumNotFound);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(Errors.Auth.UserNotFound);
    }
}
