using ArticlesService.Application.Abstractions.IntegrationEvents;
using ArticlesService.Domain.DomainEvents;
using BuildingBlocks.Contracts.IntegrationEvents;
using MediatR;

namespace ArticlesService.Application.DomainEventHandlers
{
    internal class ArticlePublishedDomainEventHandler(IIntegrationEventService eventLogService)
     : INotificationHandler<ArticlePublishedDomainEvent>
    {
        public async Task Handle(ArticlePublishedDomainEvent notification, CancellationToken cancellationToken)
        {
            var integrationEvent = new ArticlePublishedIntegrationEvent(notification.ArticleId, notification.Title, notification.AuthorId, notification.PublicationDate);

            // Сохраняем в Outbox-таблицу
            await eventLogService.SaveEventAsync(integrationEvent, cancellationToken);
        }
    }
}