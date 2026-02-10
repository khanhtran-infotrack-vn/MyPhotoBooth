using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyPhotoBooth.API;
using MyPhotoBooth.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace MyPhotoBooth.IntegrationTests.Fixtures;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;
    private readonly string _database = "myphotobooth_test";
    private readonly string _username = "postgres";
    private readonly string _password = "postgres";

    public string ConnectionString { get; private set; } = string.Empty;

    public PostgreSqlFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase(_database)
            .WithUsername(_username)
            .WithPassword(_password)
            .WithPortBinding(5432, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(5432);

        ConnectionString = $"Host={host};Port={port};Database={_database};Username={_username};Password={_password}";

        // Give the container a moment to be ready
        await Task.Delay(2000);

        // Run migrations using the API's startup
        var hostBuilder = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(ConnectionString);
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });
            });

        var hostInstance = hostBuilder.Build();

        using (var scope = hostInstance.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
