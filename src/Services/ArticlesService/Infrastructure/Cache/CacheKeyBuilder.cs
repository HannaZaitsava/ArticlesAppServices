using System.Security.Cryptography;
using System.Text;
using ArticlesService.Application.Abstractions.Caching;
using ArticlesService.Infrastructure.Cache.Settings;
using Microsoft.Extensions.Options;

namespace ArticlesService.Infrastructure.Cache
{    
    public class CacheKeyBuilder(IOptions<CacheOptions> cacheSettings) : ICacheKeyBuilder
    {
        private readonly string _apiVersion = cacheSettings.Value.ApiVersion;
        private readonly string _appPrefix = cacheSettings.Value.AppPrefix;

        public string Build(string contextName, string metadata)
        {
            if (string.IsNullOrWhiteSpace(metadata))
            {
                throw new InvalidOperationException($"Cache metadata cannot be empty for context: {contextName}");
            }

            // Кодирование метаданных в байты на стеке
            int maxByteCount = Encoding.UTF8.GetMaxByteCount(metadata.Length);
            Span<byte> metaBytesBuffer = maxByteCount <= 512 ? stackalloc byte[512] : new byte[maxByteCount];
            int writtenMetaBytes = Encoding.UTF8.GetBytes(metadata, metaBytesBuffer);

            // Хэширование SHA256 на стеке
            Span<byte> hashBuffer = stackalloc byte[32];
            SHA256.HashData(metaBytesBuffer[..writtenMetaBytes], hashBuffer);

            ReadOnlySpan<byte> hashSlice = hashBuffer[..8];

            // Переводим байты в Hex на стеке, избегая создания промежуточной строки
            Span<char> hexChars = stackalloc char[16];
            Convert.TryToHexString(hashSlice, hexChars, out _);

            return $"{_appPrefix}:{_apiVersion}:{contextName}:{hexChars}";
        }
    }
}
