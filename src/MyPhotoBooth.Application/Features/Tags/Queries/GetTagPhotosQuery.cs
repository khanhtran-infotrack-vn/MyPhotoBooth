using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Tags.Queries;

public record GetTagPhotosQuery(
    Guid TagId,
    string UserId,
    int Page = 1,
    int PageSize = 50
) : IQuery<PaginatedResult<PhotoListResponse>>;
