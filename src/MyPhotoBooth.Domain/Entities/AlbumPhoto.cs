namespace MyPhotoBooth.Domain.Entities;

public class AlbumPhoto
{
    public Guid AlbumId { get; set; }
    public Album Album { get; set; } = null!;
    
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;
    
    public DateTime AddedAt { get; set; }
    public int SortOrder { get; set; }
}
