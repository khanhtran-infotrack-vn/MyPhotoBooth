using System.ComponentModel.DataAnnotations;

namespace MyPhotoBooth.Application.Common.DTOs;

public class CreateTagRequest
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;
}

public class TagResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AddTagToPhotoRequest
{
    [Required]
    public Guid TagId { get; set; }
}
