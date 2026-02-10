using FluentValidation;
using MyPhotoBooth.Application.Common.Validators;
using MyPhotoBooth.Application.Features.Groups.Queries;

namespace MyPhotoBooth.Application.Features.Groups.Validators;

public class GetGroupQueryValidator : AbstractValidator<GetGroupQuery>
{
    public GetGroupQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty).WithMessage("Group ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
