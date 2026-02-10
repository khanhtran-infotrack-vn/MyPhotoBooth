using FluentValidation;
using MyPhotoBooth.Application.Features.Groups.Queries;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class GetGroupPhotosQueryValidator : AbstractValidator<GetGroupPhotosQuery>
{
    public GetGroupPhotosQueryValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEqual(Guid.Empty).WithMessage("Group ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
