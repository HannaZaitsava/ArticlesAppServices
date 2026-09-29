using ArticlesService.Application.Abstractions.Caching;
using ArticlesService.Infrastructure.Cache.Settings;
using BuildingBlocks.Configuration.ConfigurationValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ArticlesService.Infrastructure.Cache
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCache(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddWithFluentValidation<CacheOptions, CacheOptionsValidator>(CacheOptions.SectionName);
            // Адаптеры
            services.AddTransient<IConfigureOptions<RedisCacheOptions>, ConfigureRedisCacheOptions>();
            services.AddTransient<IConfigureOptions<HybridCacheOptions>, ConfigureHybridCacheOptions>();

            // используется в интеграционных тестах 
            //var multiplexer = ConnectionMultiplexer.Connect(cacheSettings.RedisUrl);
            //services.AddSingleton<IConnectionMultiplexer>(multiplexer);

            // Регистрируем кэш с пустым делегатом (настройку выполнит Адаптер)
            services.AddStackExchangeRedisCache(_ => { });
            services.AddHybridCache(_ => { });           
                       
            services.AddScoped<ICacheService, HybridCacheService>();
            services.AddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();

            return services;
        }
    }
}
