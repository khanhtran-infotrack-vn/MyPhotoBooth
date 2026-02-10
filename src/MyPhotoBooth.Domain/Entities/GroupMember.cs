namespace MyPhotoBooth.Domain.Entities;

public class GroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public DateTime? ContentRemovalDate { get; set; }

    // Computed properties
    public bool IsActive => !LeftAt.HasValue;
    public bool IsInGracePeriod => LeftAt.HasValue &&
        ContentRemovalDate.HasValue &&
        ContentRemovalDate.Value > DateTime.UtcNow;

    // Navigation properties
    public Group? Group { get; set; }
    public ApplicationUser? User { get; set; }
}
