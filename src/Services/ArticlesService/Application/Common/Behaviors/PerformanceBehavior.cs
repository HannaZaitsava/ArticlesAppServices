using System.Diagnostics;
using ArticlesService.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ArticlesService.Application.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private readonly IUserContext _userContext;

        public PerformanceBehavior(
            ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
            IUserContext userContext)
        {
            _logger = logger;
            _userContext = userContext;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            if (elapsedMilliseconds > 500) // Log slow requests
            {
                var requestName = typeof(TRequest).Name;
                var userId = _userContext.UserId;
                _logger.LogWarning("Long running request: Request Name: {Name}, ({ElapsedMilliseconds} ms), Current user: {UserId}, Request: {@Request}",
                    requestName, elapsedMilliseconds, userId, request);
            }
            return response;
        }
    }
}
