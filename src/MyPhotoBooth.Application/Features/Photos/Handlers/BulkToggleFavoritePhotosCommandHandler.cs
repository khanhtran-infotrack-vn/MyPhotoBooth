using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class BulkToggleFavoritePhotosCommandHandler : IRequestHandler<BulkToggleFavoritePhotosCommand, Result<BulkOperationResultDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<BulkToggleFavoritePhotosCommandHandler> _logger;

    public BulkToggleFavoritePhotosCommandHandler(
        IPhotoRepository photoRepository,
        ILogger<BulkToggleFavoritePhotosCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<BulkOperationResultDto>> Handle(
        BulkToggleFavoritePhotosCommand request,
        CancellationToken cancellationToken)
    {
        var result = new BulkOperationResultDto();
        var errors = new List<BulkOperationErrorDto>();

        // Get all photos that belong to the user
        var photos = await _photoRepository.GetByIdsAsync(request.PhotoIds, request.UserId, cancellationToken);

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

            result.SuccessCount++;
            _logger.LogInformation("Toggled favorite for photo: {PhotoId}, Favorite: {Favorite}", photoId, request.Favorite);
        }

        // Perform bulk operation
        var validPhotoIds = photos.Where(p => !errors.Any(e => e.PhotoId == p.Id.ToString())).Select(p => p.Id).ToList();

        try
        {
            if (request.Favorite)
            {
                await _photoRepository.AddToFavoritesAsync(validPhotoIds, request.UserId, cancellationToken);
            }
            else
            {
                await _photoRepository.RemoveFromFavoritesAsync(validPhotoIds, request.UserId, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to bulk toggle favorites");
            errors.Add(new BulkOperationErrorDto
            {
                PhotoId = "bulk",
                FileName = "Multiple",
                ErrorMessage = ex.Message
            });
        }

        result.Errors = errors;
        return Result.Success(result);
    }
}
