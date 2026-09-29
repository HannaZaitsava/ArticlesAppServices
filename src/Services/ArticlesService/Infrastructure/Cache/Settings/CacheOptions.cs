namespace ArticlesService.Infrastructure.Cache.Settings
{
    public sealed class CacheOptions
    {
        public const string SectionName = "CacheSettings";

        public string AppPrefix { get; set; } = "ArticleApp:";
        public string ApiVersion { get; set; } = string.Empty;
        public bool UseDistributedCache { get; set; } = true;
        public string RedisUrl { get; set; } = string.Empty;

        public bool ForceGlobalExpiration { get; set; } = false;

        public int GlobalMaxExpirationSeconds { get; set; } = 86400;
        public int ExpirationSeconds { get; set; } = 600;
        public int LocalCacheExpirationSeconds { get; set; } = 60;

        // Префикс (ключом-мутатором) в базе данных Redis, чтобы наше приложение не затирало чужие кэши.
        // Опционально для прода, обязательно для тестов.
        public string? InstanceName { get; set; } 
    }
}
