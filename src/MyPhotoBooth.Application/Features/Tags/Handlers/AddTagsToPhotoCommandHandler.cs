using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Tags.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Tags.Handlers;

public class AddTagsToPhotoCommandHandler : IRequestHandler<AddTagsToPhotoCommand, Result>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ITagRepository _tagRepository;
    private readonly ILogger<AddTagsToPhotoCommandHandler> _logger;

    public AddTagsToPhotoCommandHandler(
        IPhotoRepository photoRepository,
        ITagRepository tagRepository,
        ILogger<AddTagsToPhotoCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _tagRepository = tagRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        AddTagsToPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId, cancellationToken);
        if (photoResult.IsFailure)
            return Result.Failure(photoResult.Error);

        foreach (var tagId in request.TagIds)
        {
            var tag = await _tagRepository.GetByIdAsync(tagId, cancellationToken);
            if (tag == null || tag.UserId != request.UserId)
                continue;

            var existing = photoResult.Value.PhotoTags.FirstOrDefault(pt => pt.TagId == tagId);
            if (existing != null)
                continue;

            var photoTag = new PhotoTag
            {
                PhotoId = request.PhotoId,
                TagId = tagId
            };
            photoResult.Value.PhotoTags.Add(photoTag);
        }

        await _photoRepository.UpdateAsync(photoResult.Value, cancellationToken);

        _logger.LogInformation("Tags added to photo {PhotoId}", request.PhotoId);
        return Result.Success();
    }

    private async Task<Result<Photo>> ValidatePhotoOwnershipAsync(string userId, Guid photoId, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(photoId, cancellationToken);
        if (photo == null)
            return Result.Failure<Photo>(Errors.Photos.NotFound);
        if (photo.UserId != userId)
            return Result.Failure<Photo>(Errors.General.Unauthorized);
        return Result.Success(photo);
    }
}
