using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NotificationService.Settings;

namespace NotificationService.CommandIdempotency
{
    public class IdempotentBehavior<TRequest, TResponse>(
      IDistributedCache cache,
      IOptions<CacheOptions> cacheOptions,
      TimeProvider dateTimeProvider,
      ILogger<IdempotentBehavior<TRequest, TResponse>> logger)
      : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IIdempotentCommand
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var cacheKey = request.GetIdempotencyKey();

            // Пробуем прочитать ключ из Redis
            var isAlreadyHandled = await cache.GetStringAsync(cacheKey, cancellationToken);

            if (isAlreadyHandled is not null)
            {
                logger.LogWarning("Duplicate message detected in Redis and skipped: {CacheKey}", cacheKey);

                // Сообщение дубликат — прерываем цепочку, хендлер уведомлений не вызовется
                return default!;
            }
            
            var distributedCacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheOptions.Value.IdempotencyTtlSeconds)
            };

            // время, когда был записан ключ 
            var timestamp = dateTimeProvider.GetUtcNow().ToString("o"); // строгий международный текстовый стандарт - ISO 8601 Round-trip date/time pattern
            await cache.SetStringAsync(cacheKey, timestamp, distributedCacheEntryOptions, cancellationToken);

            try
            {
                return await next();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Business logic failed for {CacheKey}. Removing key from Redis to allow Kafka retry...", cacheKey);

                // Если отправка уведомления упала (например, отвалился SMTP-сервер или сеть), 
                // мы ДОЛЖНЫ удалить ключ из Redis. 
                // Тогда, когда Kafka пришлет это сообщение повторно, сервис сможет снова попробовать его обработать.
                await cache.RemoveAsync(cacheKey, cancellationToken);
                throw;
            }
        }
    }
}
