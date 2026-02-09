namespace MyPhotoBooth.Domain.Entities;

public class Photo
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? ExifDataJson { get; set; }
    
    // Navigation properties
    public ICollection<PhotoTag> PhotoTags { get; set; } = new List<PhotoTag>();
    public ICollection<AlbumPhoto> AlbumPhotos { get; set; } = new List<AlbumPhoto>();
}
