using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class UpdatePhotoCommandHandler : IRequestHandler<UpdatePhotoCommand, Result>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<UpdatePhotoCommandHandler> _logger;

    public UpdatePhotoCommandHandler(
        IPhotoRepository photoRepository,
        ILogger<UpdatePhotoCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(
        UpdatePhotoCommand request,
        CancellationToken cancellationToken)
    {
        var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId, cancellationToken);
        if (photoResult.IsFailure)
            return Result.Failure(photoResult.Error);

        var photo = photoResult.Value;
        photo.Description = request.Description;
        await _photoRepository.UpdateAsync(photo, cancellationToken);

        _logger.LogInformation("Photo updated: {PhotoId}", request.PhotoId);
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
