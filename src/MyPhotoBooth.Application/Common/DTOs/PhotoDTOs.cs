using System.ComponentModel.DataAnnotations;

namespace MyPhotoBooth.Application.Common.DTOs;

public class PhotoUploadResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
}

public class PhotoDetailsResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    public string? ExifData { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class PhotoListResponse
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public DateTime? CapturedAt { get; set; }
    public DateTime UploadedAt { get; set; }
    public string ThumbnailPath { get; set; } = string.Empty;
}

public class UpdatePhotoRequest
{
    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
