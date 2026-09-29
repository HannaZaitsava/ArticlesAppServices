using BuildingBlocks.IntegrationEventLogEF;

namespace ArticlesOutboxWorker.KafkaServices
{
    public interface IKafkaService : IDisposable
    {
        Task<(List<Guid> SuccessIds, List<Guid> FailedIds)> PublishBatchToKafkaAsync(IEnumerable<IntegrationEventLogEntry> messages, CancellationToken ct);
    }
}
