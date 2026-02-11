using FluentValidation;
using MyPhotoBooth.Application.Features.Tags.Queries;

namespace MyPhotoBooth.Application.Features.Tags.Validators;

public class GetTagPhotosQueryValidator : AbstractValidator<GetTagPhotosQuery>
{
    public GetTagPhotosQueryValidator()
    {
        RuleFor(x => x.TagId)
            .NotEmpty().WithMessage("TagId is required")
            .Must(BeAValidGuid).WithMessage("TagId must be a valid GUID");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100");
    }

    private bool BeAValidGuid(Guid guid)
    {
        return guid != Guid.Empty;
    }
}
