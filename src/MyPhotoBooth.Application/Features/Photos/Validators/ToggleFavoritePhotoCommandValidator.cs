using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Photos.Commands;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class ToggleFavoritePhotoCommandValidator : AbstractValidator<ToggleFavoritePhotoCommand>
{
    public ToggleFavoritePhotoCommandValidator()
    {
        RuleFor(x => x.PhotoId)
            .RequiredGuid("PhotoId");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
