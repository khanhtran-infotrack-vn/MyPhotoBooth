using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Features.Photos.Queries;
using MyPhotoBooth.Application.Interfaces;
using System.IO.Compression;

namespace MyPhotoBooth.Application.Features.Photos.Handlers;

public class BulkDownloadPhotosQueryHandler : IRequestHandler<BulkDownloadPhotosQuery, Result<BulkDownloadResultDto>>
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<BulkDownloadPhotosQueryHandler> _logger;

    public BulkDownloadPhotosQueryHandler(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        ILogger<BulkDownloadPhotosQueryHandler> logger)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Result<BulkDownloadResultDto>> Handle(
        BulkDownloadPhotosQuery request,
        CancellationToken cancellationToken)
    {
        // Get all photos that belong to the user
        var photos = await _photoRepository.GetByIdsAsync(request.PhotoIds, request.UserId, cancellationToken);

        if (photos.Count == 0)
        {
            return Result.Failure<BulkDownloadResultDto>("No valid photos found to download");
        }

        try
        {
            // Create ZIP archive in memory
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var fileNumber = 1;
                var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var photo in photos.OrderBy(p => p.UploadedAt))
                {
                    // Get file stream
                    var fileStream = await _fileStorageService.GetFileStreamAsync(photo.FilePath, cancellationToken);
                    if (fileStream == null)
                    {
                        _logger.LogWarning("Could not find file for photo: {PhotoId}", photo.Id);
                        continue;
                    }

                    // Generate unique filename
                    var entryName = GetUniqueFileName(photo.OriginalFileName, usedFileNames, fileNumber);
                    usedFileNames.Add(entryName);

                    // Add entry to ZIP
                    var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

                    using (var entryStream = entry.Open())
                    {
                        await fileStream.CopyToAsync(entryStream, cancellationToken);
                    }

                    await fileStream.DisposeAsync();
                    fileNumber++;
                }
            }

            var fileName = $"photos-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";

            return Result.Success(new BulkDownloadResultDto
            {
                FileName = fileName,
                ContentType = "application/zip",
                FileContents = memoryStream.ToArray(),
                PhotoCount = photos.Count,
                FileSize = memoryStream.Length
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ZIP archive for download");
            return Result.Failure<BulkDownloadResultDto>("Failed to create download archive");
        }
    }

    private static string GetUniqueFileName(string originalFileName, HashSet<string> usedNames, int fileNumber)
    {
        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var extension = Path.GetExtension(originalFileName);

        // Sanitize filename
        baseName = string.Join("_", baseName.Split(Path.GetInvalidFileNameChars()));

        var uniqueName = $"{baseName}{extension}";
        var counter = 1;

        while (usedNames.Contains(uniqueName))
        {
            uniqueName = $"{baseName}_{counter}{extension}";
            counter++;
        }

        // Ensure it's not too long for ZIP
        if (uniqueName.Length > 100)
        {
            uniqueName = $"photo_{fileNumber}{extension}";
        }

        return uniqueName;
    }
}
