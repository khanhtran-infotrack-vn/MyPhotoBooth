using FluentValidation;
using MyPhotoBooth.Application.Features.Tags.Queries;

namespace MyPhotoBooth.Application.Features.Tags.Validators;

public class GetTagsWithPhotoCountQueryValidator : AbstractValidator<GetTagsWithPhotoCountQuery>
{
    public GetTagsWithPhotoCountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
