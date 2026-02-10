using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Common.Pagination;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class GetPhotosQueryHandler : IRequestHandler<GetPhotosQuery, Result<PaginatedResult<PhotoListResponse>>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<GetPhotosQueryHandler> _logger;

    public GetPhotosQueryHandler(
        IPhotoRepository photoRepository,
        ILogger<GetPhotosQueryHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<PaginatedResult<PhotoListResponse>>> Handle(
        GetPhotosQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId ?? throw new UnauthorizedAccessException(Errors.General.Unauthorized);

        var skip = (request.Page - 1) * request.PageSize;

        // Get photos - repository already sorts by UploadedAt DESC by default
        var photos = await _photoRepository.GetByUserIdAsync(userId, skip, request.PageSize, cancellationToken);
        var totalCount = await _photoRepository.GetCountByUserIdAsync(userId, cancellationToken);

        // Get favorite status for all photos in batch
        var photoIds = photos.Select(p => p.Id).ToList();
        var favoriteStatus = photoIds.Any()
            ? await _photoRepository.GetFavoriteStatusAsync(photoIds, userId, cancellationToken)
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
