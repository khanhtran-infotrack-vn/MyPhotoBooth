using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Queries;

public record GetGroupQuery(
    Guid Id,
    string UserId
) : IQuery<GroupDetailsResponse>;
