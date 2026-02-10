using System.ComponentModel.DataAnnotations;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Common.DTOs;

public class CreateGroupRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class UpdateGroupRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class GroupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public int MemberCount { get; set; }
    public int ContentCount { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsDeletionScheduled { get; set; }
    public int DaysUntilDeletion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GroupDetailsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsDeletionScheduled { get; set; }
    public int DaysUntilDeletion { get; set; }
    public DateTime? DeletionProcessDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<GroupMemberResponse> Members { get; set; } = new();
    public List<GroupContentResponse> SharedContent { get; set; } = new();
}

public class GroupMemberResponse
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsInGracePeriod { get; set; }
}

public class GroupContentResponse
{
    public Guid Id { get; set; }
    public SharedContentType ContentType { get; set; }
    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }
    public string? PhotoName { get; set; }
    public string? AlbumName { get; set; }
    public string SharedByUserId { get; set; } = string.Empty;
    public DateTime SharedAt { get; set; }
    public bool IsActive { get; set; }
}

public class AddGroupMemberRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class TransferOwnershipRequest
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}

public class ShareContentToGroupRequest
{
    [Required]
    public SharedContentType ContentType { get; set; }

    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }
}
