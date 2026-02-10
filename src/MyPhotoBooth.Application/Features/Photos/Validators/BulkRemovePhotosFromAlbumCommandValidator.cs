using FluentValidation;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Photos.Commands;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class BulkRemovePhotosFromAlbumCommandValidator : AbstractValidator<BulkRemovePhotosFromAlbumCommand>
{
    public BulkRemovePhotosFromAlbumCommandValidator()
    {
        RuleFor(x => x.PhotoIds)
            .NotNull().WithMessage(Errors.Photos.PhotoIdsRequired)
            .NotEmpty().WithMessage(Errors.Photos.PhotoIdsRequired)
            .Must(x => x.Count <= 100).WithMessage(Errors.Photos.BulkOperationLimit)
            .Must(x => x.All(id => id != Guid.Empty)).WithMessage("All photo IDs must be valid");

        RuleFor(x => x.AlbumId)
            .RequiredGuid("AlbumId");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
