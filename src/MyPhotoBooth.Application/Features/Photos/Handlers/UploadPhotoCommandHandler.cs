using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using System.Text.Json;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class UploadPhotoCommandHandler : IRequestHandler<UploadPhotoCommand, Result<PhotoUploadResponse>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UploadPhotoCommandHandler> _logger;

    public UploadPhotoCommandHandler(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        IImageProcessingService imageProcessingService,
        IConfiguration configuration,
        ILogger<UploadPhotoCommandHandler> _logger)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _imageProcessingService = imageProcessingService;
        _configuration = configuration;
        this._logger = _logger;
    }

    public async Task<Result<PhotoUploadResponse>> Handle(
        UploadPhotoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var file = request.File;
        var maxFileSizeMB = int.Parse(_configuration["StorageSettings:MaxFileSizeMB"] ?? "50");

        if (file.Length > maxFileSizeMB * 1024 * 1024)
        {
            return Result.Failure<PhotoUploadResponse>(
                $"{Errors.Photos.FileTooLarge} (max {maxFileSizeMB}MB)");
        }

        using var stream = file.OpenReadStream();
        if (!_imageProcessingService.IsValidImageFile(stream, file.ContentType))
        {
            return Result.Failure<PhotoUploadResponse>(Errors.Photos.InvalidFile);
        }

        var storageKey = Guid.NewGuid().ToString();
        var storedFileName = $"{storageKey}.jpg";

        stream.Position = 0;
        var processed = await _imageProcessingService.ProcessImageAsync(stream, cancellationToken);

        var originalPath = _fileStorageService.BuildStoragePath(userId, storedFileName, false);
        var thumbnailPath = _fileStorageService.BuildStoragePath(userId, storedFileName, true);

        await _fileStorageService.SaveFileAsync(processed.OriginalStream, originalPath, cancellationToken);
        await _fileStorageService.SaveFileAsync(processed.ThumbnailStream, thumbnailPath, cancellationToken);

        processed.OriginalStream.Dispose();
        processed.ThumbnailStream.Dispose();

        DateTime? capturedAt = null;
        if (!string.IsNullOrEmpty(processed.ExifDataJson))
        {
            try
            {
                var exifDict = JsonSerializer.Deserialize<Dictionary<string, object>>(processed.ExifDataJson);
                if (exifDict != null && exifDict.ContainsKey("DateTimeOriginal"))
                {
                    if (DateTime.TryParse(exifDict["DateTimeOriginal"].ToString(), out var dt))
                    {
                        capturedAt = dt;
                    }
                }
            }
            catch { }
        }

        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = file.FileName,
            StorageKey = storageKey,
            FilePath = originalPath,
            ThumbnailPath = thumbnailPath,
            FileSize = file.Length,
            ContentType = "image/jpeg",
            Width = processed.Width,
            Height = processed.Height,
            CapturedAt = capturedAt,
            UploadedAt = DateTime.UtcNow,
            Description = request.Description,
            UserId = userId,
            ExifDataJson = processed.ExifDataJson
        };

        await _photoRepository.AddAsync(photo, cancellationToken);

        _logger.LogInformation("Photo uploaded: {PhotoId} for user {UserId}", photo.Id, userId);

        return Result.Success(new PhotoUploadResponse
        {
            Id = photo.Id,
            OriginalFileName = photo.OriginalFileName,
            FileSize = photo.FileSize,
            Width = processed.Width,
            Height = processed.Height,
            UploadedAt = photo.UploadedAt,
            Description = photo.Description
        });
    }
}
