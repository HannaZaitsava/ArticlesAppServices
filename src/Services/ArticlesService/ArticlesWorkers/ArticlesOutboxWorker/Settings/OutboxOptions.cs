namespace ArticlesOutboxWorker.Settings
{
    public class OutboxOptions
    {
        public const string SectionName = "Outbox";

        public int ActiveIntervalMs { get; init; }
        public int IdleIntervalMs { get; init; }
        public int ErrorIntervalMs { get; init; }

        public int BatchSize { get; init; } = 1000;
        public int InProgressTimeoutMs { get; init; } = 300000; // 5 мин
        public int MaxTimesSent { get; init; } = 3;
    }
}