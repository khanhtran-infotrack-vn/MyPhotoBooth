using System.ComponentModel.DataAnnotations;

namespace MyPhotoBooth.Application.Common.DTOs;

public class CreateAlbumRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class UpdateAlbumRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public Guid? CoverPhotoId { get; set; }
}

public class AlbumResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CoverPhotoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int PhotoCount { get; set; }
}

public class AlbumDetailsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CoverPhotoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<PhotoListResponse> Photos { get; set; } = new();
}

public class AddPhotoToAlbumRequest
{
    [Required]
    public Guid PhotoId { get; set; }
}
