using ArticlesService.Application.Abstractions;
using ArticlesService.Application.Common.CurrentUser;
using MediatR;

namespace ArticlesService.Application.Common.Behaviors
{
    public sealed class UserEnrichmentBehavior<TRequest, TResponse>(
    IUserContext userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IUserScopedRequest
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            request.UserContext = new CurrentUserInfo(
                    userContext.UserId ?? Guid.Empty,
                    userContext.IsAuthenticated
                );

            return await next(cancellationToken);
        }
    }
}
