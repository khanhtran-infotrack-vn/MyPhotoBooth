using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record SearchPhotosQuery(
    string SearchTerm,
    string UserId,
    int Page = 1,
    int PageSize = 50
) : IQuery<PaginatedResult<PhotoListResponse>>;
