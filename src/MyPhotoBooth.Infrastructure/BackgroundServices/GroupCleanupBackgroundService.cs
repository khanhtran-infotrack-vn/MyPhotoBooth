using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPhotoBooth.Application.Common.Configuration;
using MyPhotoBooth.Application.Interfaces;
using MyPhotoBooth.Domain.Entities;

namespace MyPhotoBooth.Infrastructure.BackgroundServices;

public class GroupCleanupBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GroupCleanupBackgroundService> _logger;
    private readonly IOptions<GroupSettings> _groupSettings;

    public GroupCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<GroupCleanupBackgroundService> logger,
        IOptions<GroupSettings> groupSettings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _groupSettings = groupSettings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Group Cleanup Background Service is starting");

        // Run immediately on startup, then on interval
        await ProcessCleanupAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var interval = TimeSpan.Parse(_groupSettings.Value.CleanupServiceInterval);
                await Task.Delay(interval, stoppingToken);

                await ProcessCleanupAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Group Cleanup Background Service");
            }
        }

        _logger.LogInformation("Group Cleanup Background Service is stopping");
    }

    private async Task ProcessCleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var groupRepository = scope.ServiceProvider.GetRequiredService<IGroupRepository>();

        _logger.LogInformation("Starting group cleanup processing");

        // Process group deletions
        await ProcessGroupDeletionsAsync(groupRepository, cancellationToken);

        // Process member content removal
        await ProcessMemberContentRemovalAsync(groupRepository, cancellationToken);

        // Send deletion reminder emails
        await SendDeletionRemindersAsync(groupRepository, cancellationToken);

        _logger.LogInformation("Group cleanup processing completed");
    }

    private async Task ProcessGroupDeletionsAsync(IGroupRepository groupRepository, CancellationToken cancellationToken)
    {
        // Get groups scheduled for deletion where deletion date has passed
        var ownedGroups = await groupRepository.GetByOwnerIdAsync("", cancellationToken); // This won't work as-is
        // We need a better way to query groups scheduled for deletion

        // For now, we'll skip this as it requires a new repository method
        // TODO: Add GetGroupsScheduledForDeletionAsync to IGroupRepository

        _logger.LogDebug("Group deletion processing: Not implemented - requires repository method");
    }

    private async Task ProcessMemberContentRemovalAsync(IGroupRepository groupRepository, CancellationToken cancellationToken)
    {
        // Get members whose content removal date has passed
        // For now, we'll skip this as it requires a new repository method
        // TODO: Add GetMembersWithExpiredContentGracePeriod to IGroupRepository

        _logger.LogDebug("Member content removal processing: Not implemented - requires repository method");
    }

    private async Task SendDeletionRemindersAsync(IGroupRepository groupRepository, CancellationToken cancellationToken)
    {
        var reminderDays = _groupSettings.Value.ReminderDays;
        var today = DateTime.UtcNow.Date;

        foreach (var days in reminderDays)
        {
            var targetDate = today.AddDays(days);

            // Find groups where deletion is scheduled for target date
            // For now, we'll skip this as it requires a new repository method
            // TODO: Add GetGroupsWithDeletionOnDate to IGroupRepository

            _logger.LogDebug("Deletion reminder processing: Day {Days} - Not implemented - requires repository method", days);
        }
    }
}
