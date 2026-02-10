namespace MyPhotoBooth.Domain.Entities;

public class Group
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Deletion scheduling fields
    public DateTime? DeletedAt { get; set; }
    public DateTime? DeletionScheduledAt { get; set; }
    public DateTime? DeletionProcessDate { get; set; }

    // Computed properties (ignored by EF Core)
    public bool IsDeleted => DeletedAt.HasValue;
    public bool IsDeletionScheduled => DeletionScheduledAt.HasValue && !DeletedAt.HasValue;
    public int DaysUntilDeletion => DeletionProcessDate.HasValue
        ? Math.Max(0, (DeletionProcessDate.Value - DateTime.UtcNow).Days)
        : 0;

    // Navigation properties
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<GroupSharedContent> SharedContent { get; set; } = new List<GroupSharedContent>();
}
