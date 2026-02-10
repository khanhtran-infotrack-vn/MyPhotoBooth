using FluentValidation;
using MyPhotoBooth.Application.Features.Photos.Queries;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class GetPhotosQueryValidator : AbstractValidator<GetPhotosQuery>
{
    public GetPhotosQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be positive");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200).WithMessage("PageSize must be between 1 and 200");

        RuleFor(x => x.SortBy)
            .IsInEnum().WithMessage("Invalid sort order");
    }
}
