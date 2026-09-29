using ArticlesService.Application.Abstractions.IntegrationEvents;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using BuildingBlocks.Contracts.IntegrationEvents;
using BuildingBlocks.IntegrationEventLogEF.Services;
using Microsoft.Extensions.Logging;

namespace ArticlesService.Infrastructure.DataAccess.Services
{
    public class IntegrationEventService(
        IIntegrationEventLogService integrationEventLogService,
        AppDbContext dbContext,
        ILogger<IntegrationEventService> logger) 
        : IIntegrationEventService
    {      
        public async Task SaveEventAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Saving integration event {IntegrationEventId} to the data base ({@IntegrationEvent})", @event.Id, @event);
                        
            await integrationEventLogService.SaveEventAsync(@event, dbContext.Database.CurrentTransaction);
        }
    }
}