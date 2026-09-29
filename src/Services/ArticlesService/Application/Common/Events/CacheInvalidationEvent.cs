using MediatR;

namespace ArticlesService.Application.Common.Events
{
    public record CacheInvalidationEvent(IReadOnlyCollection<string> Tags) : INotification;
}
