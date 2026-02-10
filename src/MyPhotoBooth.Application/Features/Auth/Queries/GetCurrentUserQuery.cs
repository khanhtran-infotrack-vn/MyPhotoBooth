using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Auth.Queries;

public record GetCurrentUserQuery(
    string UserId
) : IQuery<UserResponse>;
