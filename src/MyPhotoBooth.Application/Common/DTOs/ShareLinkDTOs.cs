using System.ComponentModel.DataAnnotations;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Application.Common.DTOs;

public class CreateShareLinkRequest
{
    [Required]
    public ShareLinkType Type { get; set; }

    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }

    [MaxLength(100)]
    public string? Password { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public bool AllowDownload { get; set; } = true;
}

public class ShareLinkResponse
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public ShareLinkType Type { get; set; }
    public Guid? PhotoId { get; set; }
    public Guid? AlbumId { get; set; }
    public string? TargetName { get; set; }
    public bool HasPassword { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool AllowDownload { get; set; }
    public string ShareUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VerifySharePasswordRequest
{
    public string? Password { get; set; }
}

public class ShareMetadataResponse
{
    public ShareLinkType Type { get; set; }
    public bool HasPassword { get; set; }
    public bool IsExpired { get; set; }
    public bool IsActive { get; set; }
}

public class SharedPhotoResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public DateTime? CapturedAt { get; set; }
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    public bool AllowDownload { get; set; }
}

public class SharedAlbumResponse
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool AllowDownload { get; set; }
    public List<SharedPhotoResponse> Photos { get; set; } = new();
}

public class SharedContentResponse
{
    public ShareLinkType Type { get; set; }
    public SharedPhotoResponse? Photo { get; set; }
    public SharedAlbumResponse? Album { get; set; }
}
