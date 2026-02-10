using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class GetPhotoFileQueryHandler : IRequestHandler<GetPhotoFileQuery, Result<PhotoFileResult>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<GetPhotoFileQueryHandler> _logger;

    public GetPhotoFileQueryHandler(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        ILogger<GetPhotoFileQueryHandler> logger)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<PhotoFileResult>> Handle(
        GetPhotoFileQuery request,
        CancellationToken cancellationToken)
    {
        var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId, cancellationToken);
        if (photoResult.IsFailure)
            return Result.Failure<PhotoFileResult>(photoResult.Error);

        var photo = photoResult.Value;
        var stream = await _fileStorageService.GetFileStreamAsync(photo.FilePath, cancellationToken);

        if (stream == null)
            return Result.Failure<PhotoFileResult>(Errors.Photos.StorageError);

        return Result.Success(new PhotoFileResult(stream, photo.ContentType, photo.OriginalFileName));
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
