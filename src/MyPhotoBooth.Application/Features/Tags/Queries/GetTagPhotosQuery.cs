using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Queries;

public record GetTagPhotosQuery(
    Guid TagId,
    string UserId
) : IQuery<List<PhotoListResponse>>;
