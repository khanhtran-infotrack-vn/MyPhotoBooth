using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.ShareLinks.Commands;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.ShareLinks.Validators;

public class CreateShareLinkCommandValidator : AbstractValidator<CreateShareLinkCommand>
{
    public CreateShareLinkCommandValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid share link type");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.BaseUrl)
            .NotEmpty().WithMessage("BaseUrl is required");

        RuleFor(x => x.PhotoId)
            .NotEmpty().WithMessage("PhotoId is required for Photo type")
            .When(x => x.Type == ShareLinkType.Photo);

        RuleFor(x => x.AlbumId)
            .NotEmpty().WithMessage("AlbumId is required for Album type")
            .When(x => x.Type == ShareLinkType.Album);

        RuleFor(x => x.ExpiresAt)
            .Must(x => !x.HasValue || x.Value > DateTime.UtcNow)
            .WithMessage("Expiration date must be in the future")
            .When(x => x.ExpiresAt.HasValue);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required when password protection is enabled")
            .When(x => !string.IsNullOrEmpty(x.Password));
    }
}
