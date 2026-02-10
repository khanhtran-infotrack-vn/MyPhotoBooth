using FluentValidation;
using MyPhotoBooth.Application.Features.Photos.Commands;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class UpdatePhotoCommandValidator : AbstractValidator<UpdatePhotoCommand>
{
    public UpdatePhotoCommandValidator()
    {
        RuleFor(x => x.PhotoId)
            .NotEqual(Guid.Empty).WithMessage("PhotoId is required");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description too long")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
