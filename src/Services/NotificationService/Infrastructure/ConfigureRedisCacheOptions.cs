using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using NotificationService.Settings;

namespace NotificationService.Infrastructure
{
    public class ConfigureRedisCacheOptions(IOptions<CacheOptions> cacheOptions) : IConfigureOptions<RedisCacheOptions>
    {
        public void Configure(RedisCacheOptions options)
        {
            options.Configuration = cacheOptions.Value.RedisUrl;
            options.InstanceName = cacheOptions.Value.InstanceName;
        }
    }
}
