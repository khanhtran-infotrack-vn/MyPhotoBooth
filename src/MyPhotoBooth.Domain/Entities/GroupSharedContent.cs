namespace MyPhotoBooth.Domain.Entities;

public class GroupSharedContent
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string SharedByUserId { get; set; } = string.Empty;
    public SharedContentType ContentType { get; set; }
    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }
    public DateTime SharedAt { get; set; }
    public DateTime? RemovedAt { get; set; }

    // Computed properties
    public bool IsActive => !RemovedAt.HasValue;

    // Navigation properties
    public Group? Group { get; set; }
    public Photo? Photo { get; set; }
    public Album? Album { get; set; }
}

public enum SharedContentType
{
    Photo = 0,
    Album = 1
}
