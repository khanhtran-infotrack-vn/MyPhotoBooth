using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record GetTimelineQuery(
    int? Year = null,
    int? Month = null,
    int Page = 1,
    int PageSize = 50,
    string? UserId = null
) : IQuery<PaginatedResult<PhotoListResponse>>;
