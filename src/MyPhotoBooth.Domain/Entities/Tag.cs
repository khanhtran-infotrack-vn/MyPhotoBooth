namespace MyPhotoBooth.Domain.Entities;

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<PhotoTag> PhotoTags { get; set; } = new List<PhotoTag>();
}
