using ArticlesService.Infrastructure.DataAccess.DbContext;
using BuildingBlocks.IntegrationEventLogEF;
using Microsoft.EntityFrameworkCore;

namespace ArticlesOutboxWorker.BackgroundServices;

public class OutboxCleanerProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxCleanerProcessor> _logger;
    private readonly TimeProvider _timeProvider;

    public OutboxCleanerProcessor(
        IServiceProvider serviceProvider, 
        ILogger<OutboxCleanerProcessor> logger,
        TimeProvider timeProvider
        )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Outbox cleaner background service has been successfully started.");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var cutOffTime = _timeProvider.GetUtcNow().AddDays(-1);

                    _logger.LogInformation("Starting scheduled cleanup of the outbox table. Deleting records older than: {Time}", cutOffTime);

                    int deletedRows = await dbContext.Set<IntegrationEventLogEntry>()
                        .Where(m => m.PublishedOnUtc != null && m.PublishedOnUtc < cutOffTime)
                        .ExecuteDeleteAsync(ct);

                    if (deletedRows > 0)
                    {
                        _logger.LogWarning("Cleanup completed. Successfully removed {Count} old records from the database.", deletedRows);
                    }
                    else
                    {
                        _logger.LogInformation("Cleanup completed. No expired outbox messages were found to delete.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the outbox table cleanup routine.");
            }

            // Sleep for 1 hour
            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }
}
