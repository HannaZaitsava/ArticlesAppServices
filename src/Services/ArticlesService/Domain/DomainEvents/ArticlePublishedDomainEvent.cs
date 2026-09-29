using ArticlesService.Domain.DomainEvents.Base;

namespace ArticlesService.Domain.DomainEvents
{
    public record ArticlePublishedDomainEvent(Guid ArticleId, string Title, Guid AuthorId, DateTimeOffset PublicationDate) : IDomainEvent;
}
