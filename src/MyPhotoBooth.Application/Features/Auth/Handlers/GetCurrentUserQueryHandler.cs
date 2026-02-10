using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Auth.Queries;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Auth.Handlers;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GetCurrentUserQueryHandler> _logger;

    public GetCurrentUserQueryHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<GetCurrentUserQueryHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result<UserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure<UserResponse>(Errors.Auth.UserNotFound);
        }

        return Result.Success(new UserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName ?? string.Empty,
            user.CreatedAt
        ));
    }
}
