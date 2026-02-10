using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class BulkRemovePhotosFromAlbumCommandHandler : IRequestHandler<BulkRemovePhotosFromAlbumCommand, Result<BulkOperationResultDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<BulkRemovePhotosFromAlbumCommandHandler> _logger;

    public BulkRemovePhotosFromAlbumCommandHandler(
        IPhotoRepository photoRepository,
        IAlbumRepository albumRepository,
        ILogger<BulkRemovePhotosFromAlbumCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result<BulkOperationResultDto>> Handle(
        BulkRemovePhotosFromAlbumCommand request,
        CancellationToken cancellationToken)
    {
        var result = new BulkOperationResultDto();
        var errors = new List<BulkOperationErrorDto>();

        // Verify album ownership
        var ownsAlbum = await _albumRepository.UserOwnsAlbumAsync(request.AlbumId, request.UserId, cancellationToken);
        if (!ownsAlbum)
        {
            return Result.Failure<BulkOperationResultDto>(Errors.Albums.NotFound);
        }

        // Get all photos that belong to the user
        var photos = await _photoRepository.GetByIdsAsync(request.PhotoIds, request.UserId, cancellationToken);

        // Get existing album photos
        var albumPhotos = await _albumRepository.GetAlbumPhotosAsync(request.AlbumId, request.PhotoIds, cancellationToken);
        var albumPhotoIds = albumPhotos.Select(ap => ap.PhotoId).ToHashSet();

        foreach (var photoId in request.PhotoIds)
        {
            var photo = photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null)
            {
                errors.Add(new BulkOperationErrorDto
                {
                    PhotoId = photoId.ToString(),
                    FileName = "Unknown",
                    ErrorMessage = "Photo not found or access denied"
                });
                result.FailedCount++;
                continue;
            }

            if (!albumPhotoIds.Contains(photoId))
            {
                errors.Add(new BulkOperationErrorDto
                {
                    PhotoId = photoId.ToString(),
                    FileName = photo.OriginalFileName,
                    ErrorMessage = "Photo not in album"
                });
                result.FailedCount++;
                continue;
            }

            result.SuccessCount++;
            _logger.LogInformation("Removed photo {PhotoId} from album {AlbumId}", photoId, request.AlbumId);
        }

        // Remove from album in bulk
        var photoIdsToRemove = photos
            .Where(p => albumPhotoIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToList();

        if (photoIdsToRemove.Count > 0)
        {
            try
            {
                await _albumRepository.RemovePhotosFromAlbumAsync(request.AlbumId, photoIdsToRemove, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove photos from album");
                errors.Add(new BulkOperationErrorDto
                {
                    PhotoId = "bulk",
                    FileName = "Multiple",
                    ErrorMessage = ex.Message
                });
            }
        }

        result.Errors = errors;
        return Result.Success(result);
    }
}
