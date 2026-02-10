using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Commands;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class BulkDeletePhotosCommandHandler : IRequestHandler<BulkDeletePhotosCommand, Result<BulkOperationResultDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<BulkDeletePhotosCommandHandler> _logger;

    public BulkDeletePhotosCommandHandler(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        ILogger<BulkDeletePhotosCommandHandler> logger)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<BulkOperationResultDto>> Handle(
        BulkDeletePhotosCommand request,
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

            try
            {
                // Delete files
                await _fileStorageService.DeleteFileAsync(photo.FilePath, cancellationToken);
                await _fileStorageService.DeleteFileAsync(photo.ThumbnailPath, cancellationToken);

                result.SuccessCount++;
                _logger.LogInformation("Photo deleted: {PhotoId}", photoId);
            }
            catch (Exception ex)
            {
                errors.Add(new BulkOperationErrorDto
                {
                    PhotoId = photoId.ToString(),
                    FileName = photo.OriginalFileName,
                    ErrorMessage = ex.Message
                });
                result.FailedCount++;
                _logger.LogError(ex, "Failed to delete photo: {PhotoId}", photoId);
            }
        }

        // Delete from database in bulk
        var photoIdsToDelete = photos.Where(p => !errors.Any(e => e.PhotoId == p.Id.ToString())).Select(p => p.Id).ToList();
        await _photoRepository.DeleteMultipleAsync(photoIdsToDelete, request.UserId, cancellationToken);

        result.Errors = errors;
        return Result.Success(result);
    }
}
