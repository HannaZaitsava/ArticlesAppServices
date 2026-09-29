using System.Diagnostics;
using ArticlesService.ArticlesAPI.Handlers;
using ArticlesService.Domain.Exceptions.Base;
using FluentValidation;

namespace ArticlesService.ArticlesAPI.Extensions.DI
{
    public static class ErrorHandlingExtensions
    {
        public static IServiceCollection AddApiErrorHandling(this IServiceCollection services)
        {
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = problemDetailsContext =>
                {
                    problemDetailsContext.ProblemDetails.Instance = $"{problemDetailsContext.HttpContext.Request.Method} {problemDetailsContext.HttpContext.Request.Path}";
                    // Добавляем общие метаданные для всех ошибок (даже 404)
                    problemDetailsContext.ProblemDetails.Extensions.TryAdd("server", Environment.MachineName);
                    problemDetailsContext.ProblemDetails.Extensions.TryAdd("requestId", problemDetailsContext.HttpContext.TraceIdentifier);
                    problemDetailsContext.ProblemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id ?? problemDetailsContext.HttpContext.TraceIdentifier);
                };
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }

        public static IApplicationBuilder UseApiErrorHandling(this IApplicationBuilder app)
        {           
            // Обработка исключений
            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                StatusCodeSelector = ex => ex switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    BaseDomainException => StatusCodes.Status400BadRequest,
                    ValidationException => StatusCodes.Status400BadRequest,
                    NotImplementedException => StatusCodes.Status501NotImplemented,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError,
                }
            });
            // Обработка пустых 404/401/403
            app.UseStatusCodePages();     

            return app;
        }
    }
}
