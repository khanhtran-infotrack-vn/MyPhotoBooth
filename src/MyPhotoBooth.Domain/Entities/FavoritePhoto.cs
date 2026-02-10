namespace MyPhotoBooth.Domain.Entities;

public class FavoritePhoto
{
    public Guid Id { get; set; }
    public Guid PhotoId { get; set; }
    public Photo Photo { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
