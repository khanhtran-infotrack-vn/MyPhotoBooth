using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyPhotoBooth.API;
using MyPhotoBooth.Application.Common;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Infrastructure.Email;
using MyPhotoBooth.Infrastructure.Persistence;
using System.Text;

namespace MyPhotoBooth.IntegrationTests.Fixtures;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear all existing configuration sources and add only test configuration
            config.Sources.Clear();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "ThisIsATestSecretKeyForIntegrationTestingOnly12345678",
                ["JwtSettings:Issuer"] = "MyPhotoBooth.Test",
                ["JwtSettings:Audience"] = "MyPhotoBooth.Test.Client",
                ["JwtSettings:AccessTokenExpirationMinutes"] = "60",
                ["JwtSettings:RefreshTokenExpirationDays"] = "7",
                ["StorageSettings:PhotosPath"] = Path.Combine(Path.GetTempPath(), "photobooth_tests"),
                ["StorageSettings:MaxFileSizeMB"] = "50"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Reconfigure JWT authentication with test settings
            var testSecretKey = "ThisIsATestSecretKeyForIntegrationTestingOnly12345678";
            var testIssuer = "MyPhotoBooth.Test";
            var testAudience = "MyPhotoBooth.Test.Client";

            // Remove the existing authentication scheme and add a new one
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            });

            services.PostConfigureAll<JwtBearerOptions>(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = testIssuer,
                    ValidAudience = testAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(testSecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Remove all AppDbContext registrations
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add test AppDbContext
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Remove all email service registrations and add mock
            var emailDescriptors = services.Where(d => d.ServiceType == typeof(IEmailService)).ToList();
            foreach (var emailDescriptor in emailDescriptors)
            {
                services.Remove(emailDescriptor);
            }
            services.AddScoped<IEmailService, MockEmailService>();

            // Remove all image processing service registrations and add mock
            var imageDescriptors = services.Where(d => d.ServiceType == typeof(IImageProcessingService)).ToList();
            foreach (var imageDescriptor in imageDescriptors)
            {
                services.Remove(imageDescriptor);
            }
            services.AddScoped<IImageProcessingService, MockImageProcessingService>();

            // Remove all file storage service registrations and add mock
            var storageDescriptors = services.Where(d => d.ServiceType == typeof(IFileStorageService)).ToList();
            foreach (var storageDescriptor in storageDescriptors)
            {
                services.Remove(storageDescriptor);
            }
            services.AddScoped<IFileStorageService, MockFileStorageService>();
        });
    }
}

public class MockEmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string htmlContent, string? plainTextContent = null, CancellationToken cancellationToken = default)
    {
        // Log instead of sending real emails
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetToken, string callbackUrl, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class MockImageProcessingService : IImageProcessingService
{
    public bool IsValidImageFile(Stream stream, string contentType)
    {
        return true; // Accept any image in tests
    }

    public Task<ProcessedImageResult> ProcessImageAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        // Create a minimal valid JPEG (1x1 pixel)
        var minimalJpg = new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01,
            0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43, 0x00, 0x01,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
            0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0xFF, 0xC0, 0x00,
            0x0B, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14,
            0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x09, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00,
            0x00, 0x01, 0xFF, 0xD9
        };

        var originalStream = new MemoryStream(minimalJpg);
        var thumbnailStream = new MemoryStream(minimalJpg);

        return Task.FromResult(new ProcessedImageResult(
            originalStream,
            thumbnailStream,
            100, // Width
            100, // Height
            null  // No EXIF data in tests
        ));
    }
}

public class MockFileStorageService : IFileStorageService
{
    // In-memory storage for tests
    private readonly Dictionary<string, byte[]> _storage = new();

    public Task<string> SaveFileAsync(Stream fileStream, string storageKey, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        _storage[storageKey] = memoryStream.ToArray();
        return Task.FromResult(storageKey);
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _storage.Remove(filePath);
        return Task.CompletedTask;
    }

    public Task<Stream?> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (_storage.TryGetValue(filePath, out var data))
        {
            return Task.FromResult<Stream?>(new MemoryStream(data));
        }
        return Task.FromResult<Stream?>(null);
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storage.ContainsKey(filePath));
    }

    public string BuildStoragePath(string userId, string fileName, bool isThumbnail = false)
    {
        var suffix = isThumbnail ? "_thumb" : "";
        var extension = Path.GetExtension(fileName);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        return $"/test/{userId}/{fileNameWithoutExt}{suffix}{extension}";
    }
}
