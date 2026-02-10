using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class BulkAddPhotosToAlbumCommandHandler : IRequestHandler<BulkAddPhotosToAlbumCommand, Result<BulkOperationResultDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IAlbumRepository _albumRepository;
    private readonly ILogger<BulkAddPhotosToAlbumCommandHandler> _logger;

    public BulkAddPhotosToAlbumCommandHandler(
        IPhotoRepository photoRepository,
        IAlbumRepository albumRepository,
        ILogger<BulkAddPhotosToAlbumCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _albumRepository = albumRepository;
        _logger = logger;
    }

    public async Task<Result<BulkOperationResultDto>> Handle(
        BulkAddPhotosToAlbumCommand request,
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
        var existingAlbumPhotos = await _albumRepository.GetAlbumPhotosAsync(request.AlbumId, request.PhotoIds, cancellationToken);
        var existingPhotoIds = existingAlbumPhotos.Select(ap => ap.PhotoId).ToHashSet();

        // Find photos not yet in album
        var newPhotoIds = photos
            .Where(p => !existingPhotoIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToList();

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

            if (existingPhotoIds.Contains(photoId))
            {
                errors.Add(new BulkOperationErrorDto
                {
                    PhotoId = photoId.ToString(),
                    FileName = photo.OriginalFileName,
                    ErrorMessage = "Photo already in album"
                });
                result.FailedCount++;
                continue;
            }

            result.SuccessCount++;
            _logger.LogInformation("Added photo {PhotoId} to album {AlbumId}", photoId, request.AlbumId);
        }

        // Add to album in bulk
        if (newPhotoIds.Count > 0)
        {
            try
            {
                await _albumRepository.AddPhotosToAlbumAsync(request.AlbumId, newPhotoIds, request.UserId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add photos to album");
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
