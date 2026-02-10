using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Groups.Queries;

public record GetGroupPhotosQuery(
    Guid GroupId,
    string UserId
) : IQuery<List<GroupContentResponse>>;
