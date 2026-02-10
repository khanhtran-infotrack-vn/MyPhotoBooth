using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class GetFavoritePhotosQueryHandler : IRequestHandler<GetFavoritePhotosQuery, Result<PaginatedResult<PhotoListResponse>>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<GetFavoritePhotosQueryHandler> _logger;

    public GetFavoritePhotosQueryHandler(
        IPhotoRepository photoRepository,
        ILogger<GetFavoritePhotosQueryHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<PhotoListResponse>>> Handle(
        GetFavoritePhotosQuery request,
        CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var photos = await _photoRepository.GetFavoritesAsync(request.UserId, skip, request.PageSize, cancellationToken);
        var totalCount = await _photoRepository.GetFavoritesCountAsync(request.UserId, cancellationToken);

        var photoList = photos.Select(p => new PhotoListResponse
        {
            Id = p.Id,
            OriginalFileName = p.OriginalFileName,
            Width = p.Width,
            Height = p.Height,
            CapturedAt = p.CapturedAt,
            UploadedAt = p.UploadedAt,
            ThumbnailPath = p.ThumbnailPath
        }).ToList();

        return Result.Success(PaginatedResult<PhotoListResponse>.Create(
            photoList, request.Page, request.PageSize, totalCount));
    }
}
