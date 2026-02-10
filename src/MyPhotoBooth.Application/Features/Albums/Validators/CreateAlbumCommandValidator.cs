using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Albums.Commands;

namespace MyPhotoBooth.Application.Features.Albums.Validators;

public class CreateAlbumCommandValidator : AbstractValidator<CreateAlbumCommand>
{
    public CreateAlbumCommandValidator()
    {
        RuleFor(x => x.Name).AlbumName();

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description too long")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
