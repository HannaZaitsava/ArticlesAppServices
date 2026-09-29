namespace BuildingBlocks.Contracts.IntegrationEvents
{
    public record ArticlePublishedIntegrationEvent(
    Guid ArticleId,
    string Title,
    Guid AuthorId,
    DateTimeOffset PublishedAt) : IntegrationEvent;
}
