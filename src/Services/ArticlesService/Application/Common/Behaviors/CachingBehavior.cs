using ArticlesService.Application.Abstractions.Caching;
using ArticlesService.Application.Common.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ArticlesService.Application.Common.Behaviors
{
    public class CachingBehavior<TRequest, TResponse>(
        ICacheService cacheService,
        ICacheKeyBuilder keyBuilder,
        ILogger<CachingBehavior<TRequest, TResponse>> logger) 
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>, ICachableRequest
    {       
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,            
            CancellationToken ct)
        {            
            // Если пользователь авторизован или кэш отключен для отдельного запроса => отключаем только ЧТЕНИЕ данных, а запись в кэш свежих данных оставляем.
            // В контексте текущей логики работы со статьями авторизованный пользователь тоже должен получать закешированные статьи.
            var bypassCacheRead = request.BypassCache; // || userContext.IsAuthenticated 

            //if (userContext.IsAuthenticated)
            //{
            //    logger.LogInformation("Bypassing cache read (but updating it) for request {RequestName} for user: {UserId}", typeof(TRequest).Name, userContext.UserId);
            //}
            //else 
            if (request.BypassCache)
            {
                logger.LogInformation("Cache bypassed for {RequestName}. Fetching from source.", typeof(TRequest).Name);
            }

            string contextName = typeof(TRequest).Name;
            string metadata = request.GetCacheKeyMetadata();
            string finalKey = keyBuilder.Build(contextName, metadata);

            logger.LogDebug("Generated cache key: {Key} for request {Type}", finalKey, typeof(TRequest).Name);

            logger.LogTrace("Fetching data from cache for key: {CacheKey}", finalKey);                      

            // Важно: внутри фабрики используем token, который придет от HybridCache (для безопасности пула потоков)
            return await cacheService.GetOrSetAsync(
                finalKey,
                factory: async (token) => await next(token), 
                request.ExpirationSeconds,
                request.LocalCacheExpirationSeconds,
                bypassCacheRead,
                request.CacheTags,
                ct);
        } 
    }
}
