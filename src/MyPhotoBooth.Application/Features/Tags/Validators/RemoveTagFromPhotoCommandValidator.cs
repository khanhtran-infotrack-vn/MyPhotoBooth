using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Tags.Commands;

namespace MyPhotoBooth.Application.Features.Tags.Validators;

public class RemoveTagFromPhotoCommandValidator : AbstractValidator<RemoveTagFromPhotoCommand>
{
    public RemoveTagFromPhotoCommandValidator()
    {
        RuleFor(x => x.PhotoId)
            .NotEmpty().WithMessage("PhotoId is required")
            .Must(BeAValidGuid).WithMessage("PhotoId must be a valid GUID");

        RuleFor(x => x.TagId)
            .NotEmpty().WithMessage("TagId is required")
            .Must(BeAValidGuid).WithMessage("TagId must be a valid GUID");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }

    private bool BeAValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}
