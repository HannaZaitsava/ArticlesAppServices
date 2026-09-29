using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;

namespace ArticlesService.Infrastructure.Cache.Settings
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
