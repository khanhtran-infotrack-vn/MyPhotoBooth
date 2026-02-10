using MediatR;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Common.Requests;

namespace MyPhotoBooth.Application.Features.Photos.Queries;

public record GetPhotosQuery(
    int Page = 1,
    int PageSize = 50,
    Guid? AlbumId = null,
    string? Search = null,
    string? UserId = null
) : IQuery<PaginatedResult<PhotoListResponse>>;
