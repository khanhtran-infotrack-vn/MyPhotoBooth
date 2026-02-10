using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class DeletePhotoCommandHandler : IRequestHandler<DeletePhotoCommand, Result>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DeletePhotoCommandHandler> _logger;

    public DeletePhotoCommandHandler(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        ILogger<DeletePhotoCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeletePhotoCommand request,
        CancellationToken cancellationToken)
    {
        var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId, cancellationToken);
        if (photoResult.IsFailure)
            return Result.Failure(photoResult.Error);

        var photo = photoResult.Value;

        await _fileStorageService.DeleteFileAsync(photo.FilePath, cancellationToken);
        await _fileStorageService.DeleteFileAsync(photo.ThumbnailPath, cancellationToken);
        await _photoRepository.DeleteAsync(request.PhotoId, cancellationToken);

        _logger.LogInformation("Photo deleted: {PhotoId}", request.PhotoId);
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
