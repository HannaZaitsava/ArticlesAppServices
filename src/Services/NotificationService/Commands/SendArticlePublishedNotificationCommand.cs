using MediatR;
using NotificationService.CommandIdempotency;

namespace NotificationService.Commands;

public record SendArticlePublishedNotificationCommand(
    Guid ArticleId,
    string Title,
    Guid AuthorId,
    DateTimeOffset PublishedAt
) : IRequest, IIdempotentCommand
{
    public string GetIdempotencyKey() => $"ArticleNotification:{ArticleId}";
}