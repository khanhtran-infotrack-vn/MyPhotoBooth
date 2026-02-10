namespace MyPhotoBooth.Application.Common.DTOs;

public record UserResponse(
    string Id,
    string Email,
    string DisplayName,
    DateTime CreatedAt
);
