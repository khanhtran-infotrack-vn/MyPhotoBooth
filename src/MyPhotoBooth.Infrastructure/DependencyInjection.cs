using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using MyPhotoBooth.Infrastructure.Identity;
using MyPhotoBooth.Infrastructure.Persistence;
using MyPhotoBooth.Infrastructure.Persistence.Repositories;
using MyPhotoBooth.Infrastructure.Storage;

namespace MyPhotoBooth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Note: Identity configuration is in Program.cs (requires ASP.NET Core context)

        // Repositories
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IShareLinkRepository, ShareLinkRepository>();

        // Auth Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();

        // Storage Services
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IImageProcessingService, ImageProcessingService>();

        return services;
    }
}
