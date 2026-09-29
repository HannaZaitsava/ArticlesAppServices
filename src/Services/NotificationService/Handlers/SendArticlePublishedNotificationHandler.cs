using MediatR;
using NotificationService.Commands;

namespace NotificationService.Handlers;

public class SendArticlePublishedNotificationHandler(ILogger<SendArticlePublishedNotificationHandler> logger)
    : IRequestHandler<SendArticlePublishedNotificationCommand>
{
    public async Task Handle(SendArticlePublishedNotificationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing notification via MediatR for author: {AuthorId}", request.AuthorId);

        // симуляция отправки сообщения (e.g., Email/Push Notification service)
        await Task.Delay(150, cancellationToken);

        logger.LogInformation("Successfully dispatched push notifications for article '{Title}' ({ArticleId})",
            request.Title, request.ArticleId);
    }
}
