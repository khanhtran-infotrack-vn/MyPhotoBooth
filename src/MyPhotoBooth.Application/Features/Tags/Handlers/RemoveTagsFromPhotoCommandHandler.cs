using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Interfaces;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class RemoveTagsFromPhotoCommandHandler : IRequestHandler<RemoveTagsFromPhotoCommand, Result>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<RemoveTagsFromPhotoCommandHandler> _logger;

    public RemoveTagsFromPhotoCommandHandler(
        IPhotoRepository photoRepository,
        ILogger<RemoveTagsFromPhotoCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RemoveTagsFromPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId, cancellationToken);
        if (photoResult.IsFailure)
            return Result.Failure(photoResult.Error);

        var photo = photoResult.Value;
        foreach (var tagId in request.TagIds)
        {
            var photoTag = photo.PhotoTags.FirstOrDefault(pt => pt.TagId == tagId);
            if (photoTag != null)
            {
                photo.PhotoTags.Remove(photoTag);
            }
        }

        await _photoRepository.UpdateAsync(photo, cancellationToken);

        _logger.LogInformation("Tags removed from photo {PhotoId}", request.PhotoId);
        return Result.Success();
    }

    private async Task<Result<Domain.Entities.Photo>> ValidatePhotoOwnershipAsync(string userId, Guid photoId, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId, cancellationToken);
        if (photo == null)
            return Result.Failure<Domain.Entities.Photo>(Errors.Photos.NotFound);
        if (photo.UserId != userId)
            return Result.Failure<Domain.Entities.Photo>(Errors.General.Unauthorized);
        return Result.Success(photo);
    }
}
