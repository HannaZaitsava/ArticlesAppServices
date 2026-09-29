using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Events;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.Common.Behaviors
{
    public class CacheInvalidationBehavior<TRequest, TResponse>(
    ICacheInvalidationContext cacheContext,
    IMediator mediator)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult 
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();
                       
            if (response is { IsSuccess: true } && cacheContext.Tags.Count > 0)
            {                
                await mediator.Publish(new CacheInvalidationEvent(cacheContext.Tags), cancellationToken);
            }

            return response;
        }
    }
}
