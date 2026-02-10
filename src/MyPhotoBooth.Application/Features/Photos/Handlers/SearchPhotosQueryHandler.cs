using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class SearchPhotosQueryHandler : IRequestHandler<SearchPhotosQuery, Result<PaginatedResult<PhotoListResponse>>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<SearchPhotosQueryHandler> _logger;

    public SearchPhotosQueryHandler(
        IPhotoRepository photoRepository,
        ILogger<SearchPhotosQueryHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<PhotoListResponse>>> Handle(
        SearchPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var photos = await _photoRepository.SearchAsync(request.UserId, request.SearchTerm, skip, request.PageSize, cancellationToken);
        var totalCount = await _photoRepository.GetSearchCountAsync(request.UserId, request.SearchTerm, cancellationToken);

        _logger.LogInformation("Search for '{SearchTerm}' by user {UserId} returned {Count} results",
            request.SearchTerm, request.UserId, totalCount);

        // Get favorite status for all photos in batch
        var photoIds = photos.Select(p => p.Id).ToList();
        var favoriteStatus = photoIds.Any()
            ? await _photoRepository.GetFavoriteStatusAsync(photoIds, request.UserId, cancellationToken)
            : new Dictionary<Guid, bool>();

        var photoList = photos.Select(p => new PhotoListResponse
        {
            Id = p.Id,
            OriginalFileName = p.OriginalFileName,
            Width = p.Width,
            Height = p.Height,
            CapturedAt = p.CapturedAt,
            UploadedAt = p.UploadedAt,
            ThumbnailPath = p.ThumbnailPath,
            IsFavorite = favoriteStatus.GetValueOrDefault(p.Id, false)
        }).ToList();

        return Result.Success(PaginatedResult<PhotoListResponse>.Create(
            photoList, request.Page, request.PageSize, totalCount));
    }
}
