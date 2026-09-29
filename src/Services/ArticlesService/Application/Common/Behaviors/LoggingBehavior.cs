using System.Diagnostics;
using ArticlesService.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ArticlesService.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : notnull
    {       
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly IUserContext _userContext;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, IUserContext userContext)
        {
            _logger = logger;
            _userContext = userContext;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _userContext.UserId;

            _logger.LogInformation("Processing request: Request Name: {Name}, Current user: {UserId}, Request: {@Request}", requestName, userId, request);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await next();

                stopwatch.Stop();
                _logger.LogInformation("Completed request: {Name} in {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Request failed: {Name} in {ElapsedMilliseconds}ms",
                    requestName, stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
