using ArticlesService.Application.Abstractions.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ArticlesService.Application.Common.Events
{
    public class CacheInvalidationHandler(
        ICacheService cacheService,
        ILogger<CacheInvalidationHandler> logger)
        : INotificationHandler<CacheInvalidationEvent>
    {
        public async Task Handle(CacheInvalidationEvent notification, CancellationToken ct)
        {            
            if (notification.Tags.Count == 0)
            {
                logger.LogWarning("Cache invalidation event received with no tags.");
                return;
            }

            try
            {
                logger.LogInformation("Invalidating cache tags: {Tags}", string.Join(", ", notification.Tags));

                await cacheService.RemoveByTagsAsync(notification.Tags, ct);

                logger.LogDebug("Cache tags invalidated successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while invalidating cache tags: {Tags}", string.Join(", ", notification.Tags));
                // Don't rethrow the exception here to avoid breaking the main process flow
            }
        }
    }
}
