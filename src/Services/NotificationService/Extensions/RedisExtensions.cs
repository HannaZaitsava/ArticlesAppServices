using BuildingBlocks.Configuration.ConfigurationValidation;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using NotificationService.Infrastructure;
using NotificationService.Settings;
using NotificationService.Validators;

namespace NotificationService.Extensions
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisCache(
            this IServiceCollection services,
            IConfiguration configuration)
        {            
            services.AddWithFluentValidation<CacheOptions, CacheOptionsValidator>(CacheOptions.SectionName);

            // Регистрируем Адаптер для ленивой настройки параметров Microsoft Redis
            services.AddTransient<IConfigureOptions<RedisCacheOptions>, ConfigureRedisCacheOptions>();

            // Регистрируем сам Redis Cache с пустым делегатом (настройку выполнит Адаптер)
            services.AddStackExchangeRedisCache(_ => { });

            return services;
        }
    }
}
