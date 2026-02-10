using FluentValidation;
using MyPhotoBooth.Application.Features.Photos.Queries;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class SearchPhotosQueryValidator : AbstractValidator<SearchPhotosQuery>
{
    public SearchPhotosQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Search term is required")
            .MinimumLength(2).WithMessage("Search term must be at least 2 characters")
            .MaximumLength(100).WithMessage("Search term cannot exceed 100 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");
    }
}
