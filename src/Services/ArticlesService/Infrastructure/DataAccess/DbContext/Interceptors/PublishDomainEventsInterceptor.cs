using ArticlesService.Domain.Entities.Base;
using MediatR;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ArticlesService.Infrastructure.DataAccess.DbContext.Interceptors
{
    public class PublishDomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IMediator _mediator;

        public PublishDomainEventsInterceptor(IMediator mediator) => _mediator = mediator;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

            // Извлекаем доменные события из трекера EF Core
            var domainEvents = context.ChangeTracker.Entries<BaseEntity>()
                .Select(x => x.Entity)
                .SelectMany(entity =>
                {
                    var events = entity.GetDomainEvents();
                    entity.ClearDomainEvents();
                    return events;
                })
                .ToList();

            // Публикуем доменные события 
            foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }

}
