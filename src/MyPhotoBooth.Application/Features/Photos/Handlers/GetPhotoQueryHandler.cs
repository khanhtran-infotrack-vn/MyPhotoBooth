using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class GetPhotoQueryHandler : IRequestHandler<GetPhotoQuery, Result<PhotoDetailsResponse>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly ILogger<GetPhotoQueryHandler> _logger;

    public GetPhotoQueryHandler(
        IPhotoRepository photoRepository,
        ILogger<GetPhotoQueryHandler> logger)
    {
        _photoRepository = photoRepository;
        _logger = logger;
    }

    public async Task<Result<PhotoDetailsResponse>> Handle(
        GetPhotoQuery request,
        CancellationToken cancellationToken)
    {
        var photoResult = await ValidatePhotoOwnershipAsync(request.UserId, request.PhotoId, cancellationToken);
        if (photoResult.IsFailure)
            return Result.Failure<PhotoDetailsResponse>(photoResult.Error);

        var photo = photoResult.Value;

        return Result.Success(new PhotoDetailsResponse
        {
            Id = photo.Id,
            OriginalFileName = photo.OriginalFileName,
            FileSize = photo.FileSize,
            Width = photo.Width,
            Height = photo.Height,
            CapturedAt = photo.CapturedAt,
            UploadedAt = photo.UploadedAt,
            Description = photo.Description,
            ExifData = photo.ExifDataJson,
            Tags = photo.PhotoTags.Select(pt => pt.Tag.Name).ToList()
        });
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
