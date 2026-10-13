using System.Diagnostics;
using System.Reflection;
using ArticlesService.Application.Abstractions.Caching;
using ArticlesService.Domain.Result;
using ArticlesService.Infrastructure.Cache.Settings;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ArticlesService.Infrastructure.Cache
{    
    public class HybridCacheService(
        HybridCache hybridCache, 
        ILogger<HybridCacheService> logger,
        IOptionsMonitor<CacheOptions> cacheSettings) : ICacheService
    {   
        public async ValueTask<TResponse> GetOrSetAsync<TResponse>(
        string requestCacheKey,
        Func<CancellationToken, ValueTask<TResponse>> factory,
        int? requestExpirationSeconds,
        int? requestLocalCacheExpirationSeconds,
        bool bypassCacheRead,
        IEnumerable<string>? tags = null,
        CancellationToken ct = default)
        {
            var currentSettings = cacheSettings.CurrentValue;

            var hybridCacheEntryOptions = BuildOptions(currentSettings, requestExpirationSeconds, requestLocalCacheExpirationSeconds, bypassCacheRead);

            var sw = Stopwatch.StartNew();
            bool isHit = true; 

            try
            {
                // Кэшировать Result вместе со статусом успеха нельзя. Нужно кэшировать только чистые данные.
                // Кэшировать нужно только успешно полученные данные.                 
                if (typeof(IResult).IsAssignableFrom(typeof(TResponse)))
                {
                    // Получаем тип ЧИСТЫХ ДАННЫХ (например, PagedResult<DTO>)
                    var dataType = typeof(TResponse).GetGenericArguments()[0];

                    try
                    {
                        // Вызываем HybridCache только для чистых данных DTO, минуя сам класс Result
                        // Используется State Pattern для передачи фабрики, чтобы не было замыкания и лишней аллокации
                        var cachedData = await hybridCache.GetOrCreateAsync(
                            requestCacheKey,
                            state: factory,
                            factory: static async (state, token) =>
                            {
                                IResult executionResult;
                                var originalFactory = state;
                                try
                                {      
                                    // В High-Load лучше использовать token от HybridCache, 
                                    // чтобы отмена запроса одним пользователем не ломала сборку кэша для остальных:
                                    // пользователь отменил запрос на получение данных, но кэш все равно заполнится, чтобы другие пользователи,
                                    // кототрые отправили такой же запрос и теперь стоят заблокированные в очереди за закэшированными данными, не получили отказ кэша
                                    executionResult = (IResult)(await originalFactory(token))!;
                                }
                                catch (Exception ex) // обработка ошибки ТОЛЬКО бизнес-логики
                                {
                                    throw new FactoryException(ex);
                                }

                                if (!executionResult.IsSuccess)
                                {
                                    throw new CacheBypassException(executionResult!);
                                }

                                // Сериализуем успешный Result в строку для сохранения в Redis/InMemory
                                return System.Text.Json.JsonSerializer.Serialize(executionResult.RawValue);
                            },
                            hybridCacheEntryOptions,
                            tags,
                            ct // Внешний токен отменяет только ОЖИДАНИЕ ответа этим пользователем
                        );

                        LogCacheResult(isHit, requestCacheKey, sw.ElapsedMilliseconds);

                        // Превращаем строку обратно в PagedResult<...>
                        var deserializedData = System.Text.Json.JsonSerializer.Deserialize(cachedData, dataType);

                        ///TODO: избавиться от рефлексии
                        // Оборачиваем десериализованный объект в Result                        
                        return (TResponse)typeof(TResponse)
                            .GetMethod("Success", BindingFlags.Public | BindingFlags.Static)!
                            .Invoke(null, [deserializedData])!;
                    }
                    catch (FactoryException ex)
                    {
                        // обрабатываем ошибку из хендлера, чтобы глобальный Middleware получил оригинал                        
                        throw ex.InnerException ?? ex;
                    }
                    catch (CacheBypassException ex)
                    {
                        return (TResponse)ex.ResultObject;
                    }
                    catch (Exception ex) 
                    {
                        // Ошибка ИНФРАСТРУКТУРЫ КЭША
                        logger.LogError(ex, "Cache read failure for key: {Key}. Falling back directly to database.", requestCacheKey);
                        return await factory(ct);
                    }
                }

                // Фоллбек для стандартных типов (без IResult)
                var standardResult = await hybridCache.GetOrCreateAsync(
                    requestCacheKey,
                    state: (factory, ct),
                    factory: static async (state, token) =>
                    {
                        var (originalFactory, originalCt) = state;
                        return await originalFactory(token);
                    },
                    hybridCacheEntryOptions,
                    tags,
                    ct
                );

                LogCacheResult(isHit, requestCacheKey, sw.ElapsedMilliseconds);
                return standardResult;
            }
            catch (CacheBypassException ex)
            {
                return (TResponse)ex.ResultObject;
            }
            finally
            {
                sw.Stop();
            }
        }       

        private void LogCacheResult(bool isHit, string key, long elapsedMs)
        {
            if (isHit) logger.LogDebug("Cache HIT: {Key}", key);
            else logger.LogInformation("Cache MISS: {Key}. Fetched from source in {Elapsed}ms.", key, elapsedMs);
        }

        private sealed class CacheBypassException(object resultObject) : Exception
        {
            public object ResultObject { get; } = resultObject;
        }

        private HybridCacheEntryOptions BuildOptions(CacheOptions cacheSettings, int? exp, int? localExp, bool bypassCacheRead)
        {
            // Определяем TTL для L2 (Redis) (Приоритет: ForceGlobal > Request)
            var finalExpiration = exp.HasValue
                ? cacheSettings.ForceGlobalExpiration ? cacheSettings.ExpirationSeconds : exp.Value
                : cacheSettings.ExpirationSeconds;

            // Определяем TTL для L1 (In-Memory)
            var finalLocalExpiration = localExp.HasValue
                ? cacheSettings.ForceGlobalExpiration ? cacheSettings.LocalCacheExpirationSeconds : localExp.Value
                : cacheSettings.LocalCacheExpirationSeconds;

            var flags = bypassCacheRead
                ? HybridCacheEntryFlags.DisableLocalCacheRead | HybridCacheEntryFlags.DisableDistributedCacheRead
                : HybridCacheEntryFlags.None;

            return new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(finalExpiration),
                LocalCacheExpiration = TimeSpan.FromSeconds(finalLocalExpiration),
                //Flags = HybridCacheEntryFlags.DisableLocalCache, // TODO временно отключает L1, чтобы протестировать L2
                Flags = flags
            };
        }

        public async ValueTask RemoveByTagAsync(string tag, CancellationToken ct = default)
        {
            await hybridCache.RemoveByTagAsync(tag, ct);
        }

        public async ValueTask RemoveByTagsAsync(IEnumerable<string> tags, CancellationToken ct = default)
        {
            await hybridCache.RemoveByTagAsync(tags, ct);
        }

        public async ValueTask RemoveByKeyAsync(string key, CancellationToken ct = default)
        {
            var currentSettings = cacheSettings.CurrentValue;

            await hybridCache.RemoveAsync($"{currentSettings.AppPrefix}{key}", ct);
        }

        public async ValueTask RemoveByKeysAsync(IEnumerable<string> keys, CancellationToken ct = default)
        {
            var currentSettings = cacheSettings.CurrentValue;

            var prefixedKeys = keys.Select(key => $"{currentSettings.AppPrefix}{key}");
            await hybridCache.RemoveAsync(prefixedKeys, ct);
        }        
    }
}
