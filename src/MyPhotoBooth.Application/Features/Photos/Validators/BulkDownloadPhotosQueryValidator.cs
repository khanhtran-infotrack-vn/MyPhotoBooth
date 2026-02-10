using FluentValidation;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Features.Photos.Queries;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class BulkDownloadPhotosQueryValidator : AbstractValidator<BulkDownloadPhotosQuery>
{
    public BulkDownloadPhotosQueryValidator()
    {
        RuleFor(x => x.PhotoIds)
            .NotNull().WithMessage(Errors.Photos.PhotoIdsRequired)
            .NotEmpty().WithMessage(Errors.Photos.PhotoIdsRequired)
            .Must(x => x.Count <= 50).WithMessage(Errors.Photos.DownloadLimit)
            .Must(x => x.All(id => id != Guid.Empty)).WithMessage("All photo IDs must be valid");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
