using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;

namespace ArticlesService.Infrastructure.Cache.Settings
{
    public class ConfigureHybridCacheOptions(IOptions<CacheOptions> cacheOptions) : IConfigureOptions<HybridCacheOptions>
    {
        public void Configure(HybridCacheOptions options)
        {
           options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromSeconds(cacheOptions.Value.ExpirationSeconds),
                LocalCacheExpiration = TimeSpan.FromSeconds(cacheOptions.Value.LocalCacheExpirationSeconds)
            };
        }
    }
}
