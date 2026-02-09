using Microsoft.AspNetCore.Identity;

namespace MyPhotoBooth.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
