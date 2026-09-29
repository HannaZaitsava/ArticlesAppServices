using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Contracts.IntegrationEvents;

namespace BuildingBlocks.IntegrationEventLogEF.Services;

public interface IIntegrationEventLogService
{
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId);
    Task<IEnumerable<IntegrationEventLogEntry>> RetrieveAndLockEventLogsAsync(int batchSize, int MaxTimesSent, CancellationToken ct = default);

    Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction? transaction);
    Task MarkEventAsPublishedAsync(Guid eventId);
    Task MarkEventAsInProgressAsync(Guid eventId);
    Task MarkEventAsFailedAsync(Guid eventId, int maxTimesSent);

    Task BatchMarkEventAsInProgressedAsync(IEnumerable<Guid> eventIds);
    Task BatchMarkEventAsPublishedAsync(IEnumerable<Guid> eventIds);
    Task BatchMarkEventAsFailedAsync(IEnumerable<Guid> eventIds, int maxTimesSent);
    Task ResetStaleInProgressEventsAsync(TimeSpan timeout);
}
