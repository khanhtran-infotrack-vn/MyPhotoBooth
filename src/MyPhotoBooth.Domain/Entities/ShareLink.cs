namespace MyPhotoBooth.Domain.Entities;

public enum ShareLinkType
{
    Photo = 0,
    Album = 1
}

public class ShareLink
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public ShareLinkType Type { get; set; }
    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool AllowDownload { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Computed properties
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;

    // Navigation properties
    public Photo? Photo { get; set; }
    public Album? Album { get; set; }
}
