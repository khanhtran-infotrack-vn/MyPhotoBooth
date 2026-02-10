using FluentValidation;
using MyPhotoBooth.Application.Features.Groups.Queries;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class GetGroupsQueryValidator : AbstractValidator<GetGroupsQuery>
{
    public GetGroupsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
