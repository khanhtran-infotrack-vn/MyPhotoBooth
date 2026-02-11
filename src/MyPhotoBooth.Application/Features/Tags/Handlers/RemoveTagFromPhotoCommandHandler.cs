using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Interfaces;
using CSharpFunctionalExtensions;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class RemoveTagFromPhotoCommandHandler : IRequestHandler<RemoveTagFromPhotoCommand, Result>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<RemoveTagFromPhotoCommandHandler> _logger;

    public RemoveTagFromPhotoCommandHandler(
        IPhotoRepository photoRepository,
        ILogger<RemoveTagFromPhotoCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        RemoveTagFromPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId, cancellationToken);
        if (photoResult.IsFailure)
            return Result.Failure(photoResult.Error);

        var photo = photoResult.Value;

        var photoTag = photo.PhotoTags.FirstOrDefault(pt => pt.TagId == request.TagId);
        if (photoTag != null)
        {
            photo.PhotoTags.Remove(photoTag);
            await _photoRepository.UpdateAsync(photo, cancellationToken);
            _logger.LogInformation(
                "Tag {TagId} removed from photo {PhotoId}",
                request.TagId,
                request.PhotoId);
        }
        else
        {
            _logger.LogInformation(
                "Tag {TagId} was not associated with photo {PhotoId}, no removal needed",
                request.TagId,
                request.PhotoId);
        }

        return Result.Success();
    }

    private async Task<Result<Domain.Entities.Photo>> ValidatePhotoOwnershipAsync(
        string userId,
        Guid photoId,
        CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId, cancellationToken);
        if (photo == null)
            return Result.Failure<Domain.Entities.Photo>(Errors.Photos.NotFound);
        if (photo.UserId != userId)
            return Result.Failure<Domain.Entities.Photo>(Errors.General.Unauthorized);
        return Result.Success(photo);
    }
}
