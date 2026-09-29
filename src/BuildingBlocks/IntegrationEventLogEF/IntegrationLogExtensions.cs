namespace BuildingBlocks.IntegrationEventLogEF;

public static class IntegrationLogExtensions
{
    public static void UseIntegrationEventLogs(this ModelBuilder builder)
    {
        builder.Entity<IntegrationEventLogEntry>(builder =>
        {
            builder.ToTable("IntegrationEventLog");

            builder.HasKey(e => e.EventId);

            // индекс для поиска необработанных сообщений
            builder.HasIndex(e => new { e.State, e.CreatedOnUtc })
                .HasDatabaseName("IX_IntegrationEventLog_PendingQueue")
                .HasFilter($"\"State\" = {(int)EventStateEnum.NotPublished}")
                .IncludeProperties(e => e.EventId); // можно не указывать ,т.к. EventId - первичный ключ                     

            // Индекс для поиска строк, которые пора удалить
            builder.HasIndex(m => m.PublishedOnUtc)
                .HasDatabaseName("IX_IntegrationEventLog_PublishedOnUtc");
        });
    }
}
