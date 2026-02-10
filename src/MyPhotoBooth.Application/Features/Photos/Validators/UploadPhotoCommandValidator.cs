using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Photos.Commands;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class UploadPhotoCommandValidator : AbstractValidator<UploadPhotoCommand>
{
    public UploadPhotoCommandValidator()
    {
        RuleFor(x => x.File)
            .ImageFile(maxSizeMB: 50);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description too long")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
