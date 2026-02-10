using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Albums.Commands;

namespace MyPhotoBooth.Application.Features.Albums.Validators;

public class AddPhotosToAlbumCommandValidator : AbstractValidator<AddPhotosToAlbumCommand>
{
    public AddPhotosToAlbumCommandValidator()
    {
        RuleFor(x => x.AlbumId)
            .NotEqual(Guid.Empty).WithMessage("AlbumId is required");

        RuleFor(x => x.PhotoIds)
            .NotEmpty().WithMessage("At least one photo ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleForEach(x => x.PhotoIds)
            .NotEqual(Guid.Empty).WithMessage("PhotoId cannot be empty");
    }
}
