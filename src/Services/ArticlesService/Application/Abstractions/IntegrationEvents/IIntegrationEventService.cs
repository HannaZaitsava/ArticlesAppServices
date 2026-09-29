using BuildingBlocks.Contracts.IntegrationEvents;

namespace ArticlesService.Application.Abstractions.IntegrationEvents
{
    public interface IIntegrationEventService
    {
        Task SaveEventAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);
    }
}