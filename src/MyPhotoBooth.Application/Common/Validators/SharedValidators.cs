using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace MyPhotoBooth.Application.Common.Validators;

public static class SharedValidators
{
    private static readonly HashSet<string> ValidImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp"
    };

    public static IRuleBuilderOptions<T, string> Email<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email too long");
    }

    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches("[0-9]").WithMessage("Password must contain digit");
    }

    public static IRuleBuilderOptions<T, IFormFile> ImageFile<T>(this IRuleBuilder<T, IFormFile> ruleBuilder, int maxSizeMB = 50)
    {
        return ruleBuilder
            .NotNull().WithMessage("File is required")
            .Must(f => f != null && f.Length > 0).WithMessage("File is empty")
            .Must(f => f != null && f.Length <= maxSizeMB * 1024 * 1024)
                .WithMessage($"File size exceeds {maxSizeMB}MB")
            .Must(f => f != null && ValidImageContentTypes.Contains(f.ContentType))
                .WithMessage("File must be an image (JPEG, PNG, GIF, WebP, or BMP)");
    }

    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(12).WithMessage("Password must be at least 12 characters")
            .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain special character");
    }

    public static IRuleBuilderOptions<T, string> UserName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username too long")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, underscores, and hyphens");
    }

    public static IRuleBuilderOptions<T, string> AlbumName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Album name is required")
            .MinimumLength(1).WithMessage("Album name is required")
            .MaximumLength(200).WithMessage("Album name too long");
    }

    public static IRuleBuilderOptions<T, string> TagName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Tag name is required")
            .MinimumLength(1).WithMessage("Tag name is required")
            .MaximumLength(100).WithMessage("Tag name too long")
            .Matches("^[a-zA-Z0-9\\s-_]+$").WithMessage("Tag name can only contain letters, numbers, spaces, hyphens, and underscores");
    }

    public static IRuleBuilderOptions<T, Guid> RequiredGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder, string propertyName)
    {
        return ruleBuilder
            .NotEqual(Guid.Empty).WithMessage($"{propertyName} is required");
    }
}
