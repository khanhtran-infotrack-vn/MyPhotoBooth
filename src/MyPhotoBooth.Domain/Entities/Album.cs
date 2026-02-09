namespace MyPhotoBooth.Domain.Entities;

public class Album
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CoverPhotoId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<AlbumPhoto> AlbumPhotos { get; set; } = new List<AlbumPhoto>();
}
