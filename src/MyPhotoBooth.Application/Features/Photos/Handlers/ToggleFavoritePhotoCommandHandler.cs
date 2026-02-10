using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class ToggleFavoritePhotoCommandHandler : IRequestHandler<ToggleFavoritePhotoCommand, Result<bool>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<ToggleFavoritePhotoCommandHandler> _logger;

    public ToggleFavoritePhotoCommandHandler(
        IPhotoRepository photoRepository,
        ILogger<ToggleFavoritePhotoCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ToggleFavoritePhotoCommand request,
        CancellationToken cancellationToken)
    {
        // Verify photo exists and belongs to user
        var photo = await _photoRepository.GetByIdAsync(request.PhotoId, cancellationToken);

        if (photo == null || photo.UserId != request.UserId)
            return Result.Failure<bool>(Errors.Photos.NotFound);

        // Check if already favorited
        var isFavorite = await _photoRepository.IsFavoriteAsync(request.PhotoId, request.UserId, cancellationToken);

        if (isFavorite)
        {
            // Remove from favorites
            await _photoRepository.ToggleFavoriteAsync(request.PhotoId, request.UserId, cancellationToken);
            _logger.LogInformation("Removed photo from favorites: {PhotoId} by user {UserId}", request.PhotoId, request.UserId);
            return Result.Success(false);
        }
        else
        {
            // Add to favorites
            await _photoRepository.ToggleFavoriteAsync(request.PhotoId, request.UserId, cancellationToken);
            _logger.LogInformation("Added photo to favorites: {PhotoId} by user {UserId}", request.PhotoId, request.UserId);
            return Result.Success(true);
        }
    }
}
