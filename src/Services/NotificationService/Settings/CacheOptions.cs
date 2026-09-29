namespace NotificationService.Settings
{
    public record CacheOptions
    {
        public const string SectionName = "Redis";

        public string RedisUrl { get; init; } = string.Empty;
        public string InstanceName { get; init; } = "NotificationInbox:";
        public int IdempotencyTtlSeconds { get; init; } = 1;
    }
}
