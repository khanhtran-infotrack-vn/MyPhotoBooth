using FluentValidation;
using MyPhotoBooth.Application.Features.Photos.Queries;

namespace MyPhotoBooth.Application.Features.Photos.Validators;

public class GetFavoritePhotosQueryValidator : AbstractValidator<GetFavoritePhotosQuery>
{
    public GetFavoritePhotosQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");
    }
}
