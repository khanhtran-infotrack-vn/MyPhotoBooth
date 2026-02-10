using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyPhotoBooth.Application.Common.Behaviors;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;
using MyPhotoBooth.Infrastructure.Common.Behaviors;
using MyPhotoBooth.Infrastructure.Email;
using MyPhotoBooth.Infrastructure.Identity;
using MyPhotoBooth.Infrastructure.Persistence;
using MyPhotoBooth.Infrastructure.Persistence.Repositories;
using MyPhotoBooth.Infrastructure.Storage;
using System.Reflection;

namespace MyPhotoBooth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Note: Identity configuration is in Program.cs (requires ASP.NET Core context)

        // MediatR - Register from both Infrastructure and Application assemblies
        var infrastructureAssembly = Assembly.GetExecutingAssembly();
        var applicationAssembly = typeof(MyPhotoBooth.Application.Interfaces.IPhotoRepository).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(infrastructureAssembly);
            cfg.RegisterServicesFromAssembly(applicationAssembly);
        });

        // FluentValidation - Register from both assemblies
        services.AddValidatorsFromAssembly(infrastructureAssembly);
        services.AddValidatorsFromAssembly(applicationAssembly);

        // Pipeline behaviors (order matters!)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Repositories
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IAlbumRepository, AlbumRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IShareLinkRepository, ShareLinkRepository>();

        // Auth Services
        services.AddScoped<ITokenService, TokenService>();

        // Email Service
        services.AddScoped<ITemplateEngine, TemplateEngine>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpClient();

        // Storage Services
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IImageProcessingService, ImageProcessingService>();

        return services;
    }
}
