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
                ["JwtSettings:RefreshTokenExpirationDays"] = "7"
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
