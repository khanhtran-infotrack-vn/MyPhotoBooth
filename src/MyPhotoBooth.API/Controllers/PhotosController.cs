using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPhotoBooth.Application.Common.DTOs;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly IPhotoRepository _photoRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IConfiguration _configuration;

    public PhotosController(
        IPhotoRepository photoRepository,
        IFileStorageService fileStorageService,
        IImageProcessingService imageProcessingService,
        IConfiguration configuration)
    {
        _photoRepository = photoRepository;
        _fileStorageService = fileStorageService;
        _imageProcessingService = imageProcessingService;
        _configuration = configuration;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) 
        ?? throw new UnauthorizedAccessException("User ID not found");

    [HttpPost]
    public async Task<IActionResult> UploadPhoto(IFormFile file, [FromForm] string? description, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file uploaded" });
        }

        var maxFileSizeMB = int.Parse(_configuration["StorageSettings:MaxFileSizeMB"] ?? "50");
        if (file.Length > maxFileSizeMB * 1024 * 1024)
        {
            return BadRequest(new { message = $"File size exceeds {maxFileSizeMB}MB limit" });
        }

        using var stream = file.OpenReadStream();
        if (!_imageProcessingService.IsValidImageFile(stream, file.ContentType))
        {
            return BadRequest(new { message = "Invalid image file" });
        }

        var userId = GetUserId();
        var storageKey = Guid.NewGuid().ToString();
        var fileName = $"{storageKey}.jpg";

        // Process image
        stream.Position = 0;
        var processed = await _imageProcessingService.ProcessImageAsync(stream, cancellationToken);

        // Save files
        var originalPath = _fileStorageService.BuildStoragePath(userId, fileName, false);
        var thumbnailPath = _fileStorageService.BuildStoragePath(userId, fileName, true);

        await _fileStorageService.SaveFileAsync(processed.OriginalStream, originalPath, cancellationToken);
        await _fileStorageService.SaveFileAsync(processed.ThumbnailStream, thumbnailPath, cancellationToken);

        processed.OriginalStream.Dispose();
        processed.ThumbnailStream.Dispose();

        // Extract captured date from EXIF if available
        DateTime? capturedAt = null;
        if (!string.IsNullOrEmpty(processed.ExifDataJson))
        {
            try
            {
                var exifDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(processed.ExifDataJson);
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

        // Create photo record
        var photo = new Photo
        {
            Id = Guid.NewGuid(),
            OriginalFileName = file.FileName,
            StorageKey = storageKey,
            FilePath = originalPath,
            ThumbnailPath = thumbnailPath,
            FileSize = file.Length,
            ContentType = "image/jpeg",
            CapturedAt = capturedAt,
            UploadedAt = DateTime.UtcNow,
            Description = description,
            UserId = userId,
            ExifDataJson = processed.ExifDataJson
        };

        await _photoRepository.AddAsync(photo, cancellationToken);

        return Ok(new PhotoUploadResponse
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

    [HttpGet]
    public async Task<IActionResult> ListPhotos([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var skip = (page - 1) * pageSize;

        var photos = await _photoRepository.GetByUserIdAsync(userId, skip, pageSize, cancellationToken);
        var photoList = photos.Select(p => new PhotoListResponse
        {
            Id = p.Id,
            OriginalFileName = p.OriginalFileName,
            CapturedAt = p.CapturedAt,
            UploadedAt = p.UploadedAt,
            ThumbnailPath = p.ThumbnailPath
        }).ToList();

        // Note: Total count calculation omitted for brevity - would need separate repo method
        return Ok(new PaginatedResponse<PhotoListResponse>
        {
            Items = photoList,
            Page = page,
            PageSize = pageSize,
            TotalCount = photoList.Count,
            TotalPages = 1
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPhoto(Guid id, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(id, cancellationToken);
        
        if (photo == null)
        {
            return NotFound();
        }

        if (photo.UserId != GetUserId())
        {
            return Forbid();
        }

        return Ok(new PhotoDetailsResponse
        {
            Id = photo.Id,
            OriginalFileName = photo.OriginalFileName,
            FileSize = photo.FileSize,
            Width = 0, // Would need to store or recalculate
            Height = 0,
            CapturedAt = photo.CapturedAt,
            UploadedAt = photo.UploadedAt,
            Description = photo.Description,
            ExifData = photo.ExifDataJson,
            Tags = photo.PhotoTags.Select(pt => pt.Tag.Name).ToList()
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePhoto(Guid id, [FromBody] UpdatePhotoRequest request, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(id, cancellationToken);
        
        if (photo == null)
        {
            return NotFound();
        }

        if (photo.UserId != GetUserId())
        {
            return Forbid();
        }

        photo.Description = request.Description;
        await _photoRepository.UpdateAsync(photo, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(Guid id, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(id, cancellationToken);
        
        if (photo == null)
        {
            return NotFound();
        }

        if (photo.UserId != GetUserId())
        {
            return Forbid();
        }

        // Delete files
        await _fileStorageService.DeleteFileAsync(photo.FilePath, cancellationToken);
        await _fileStorageService.DeleteFileAsync(photo.ThumbnailPath, cancellationToken);

        // Delete database record
        await _photoRepository.DeleteAsync(id, cancellationToken);

        return NoContent();
    }

    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetPhotoFile(Guid id, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(id, cancellationToken);
        
        if (photo == null)
        {
            return NotFound();
        }

        if (photo.UserId != GetUserId())
        {
            return Forbid();
        }

        var stream = await _fileStorageService.GetFileStreamAsync(photo.FilePath, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, photo.ContentType, photo.OriginalFileName);
    }

    [HttpGet("{id}/thumbnail")]
    public async Task<IActionResult> GetPhotoThumbnail(Guid id, CancellationToken cancellationToken)
    {
        var photo = await _photoRepository.GetByIdAsync(id, cancellationToken);
        
        if (photo == null)
        {
            return NotFound();
        }

        if (photo.UserId != GetUserId())
        {
            return Forbid();
        }

        var stream = await _fileStorageService.GetFileStreamAsync(photo.ThumbnailPath, cancellationToken);
        if (stream == null)
        {
            return NotFound();
        }

        return File(stream, "image/jpeg");
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> GetTimeline([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var skip = (page - 1) * pageSize;

        DateTime? fromDate = null;
        DateTime? toDate = null;

        if (year.HasValue && month.HasValue)
        {
            fromDate = new DateTime(year.Value, month.Value, 1);
            toDate = fromDate.Value.AddMonths(1);
        }
        else if (year.HasValue)
        {
            fromDate = new DateTime(year.Value, 1, 1);
            toDate = fromDate.Value.AddYears(1);
        }

        var photos = await _photoRepository.GetTimelineAsync(userId, fromDate, toDate, skip, pageSize, cancellationToken);
        var photoList = photos.Select(p => new PhotoListResponse
        {
            Id = p.Id,
            OriginalFileName = p.OriginalFileName,
            CapturedAt = p.CapturedAt,
            UploadedAt = p.UploadedAt,
            ThumbnailPath = p.ThumbnailPath
        }).ToList();

        return Ok(new PaginatedResponse<PhotoListResponse>
        {
            Items = photoList,
            Page = page,
            PageSize = pageSize,
            TotalCount = photoList.Count,
            TotalPages = 1
        });
    }
}
