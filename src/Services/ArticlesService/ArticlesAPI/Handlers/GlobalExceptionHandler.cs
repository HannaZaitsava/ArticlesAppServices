using System.Diagnostics;
using ArticlesService.ArticlesAPI.Extensions;
using ArticlesService.Domain.Exceptions.Base;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArticlesService.ArticlesAPI.Handlers
{
    public class GlobalExceptionHandler(
      IProblemDetailsService problemDetailsService,
      ILogger<GlobalExceptionHandler> logger,
      IHostEnvironment env) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            logger.LogError(exception, 
                "HTTP {Method} {Path} failed to execute",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var businessEx = exception is BaseDomainException;
            var isDevelopment = env.IsDevelopment();
                       
            var problemDetails = new ProblemDetails
            {
               // Status = statusCode,
                Title = businessEx ? "Business Rule Violation" : "Server Error",
                //Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",// - настроено в DI
                // В Production скрываем Detail для системных ошибок
                Detail = (businessEx || isDevelopment)
                    ? exception.GetAllMessages()
                    : "An unexpected error occurred."
            };                        
           
            if (isDevelopment)
            {
                // TryAdd() чтобы не затереть ключи, добавленные при вызове AddProblemDetails() в DI
                problemDetails.Extensions.TryAdd("debug_stackTrace", SplitStackTrace(exception.StackTrace));
            }
                     
           


            // Используем сервис для записи ответа
            var problemDetailsContext = new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            };

            /*
            Core Behavior & ConstraintsJSON Focus: 
            1. The implementation targets JSON-based error payloads compliant with RFC 7807.            
            2. Content Negotiation: The CanWrite method returns false if the incoming request contains an Accept header 
            that is explicitly non-JSON (e.g., text/plain), skipping the writer.
             */
            // Работает только если клиентская сторона может принимать ответы типа application/json
            var isHandled = await problemDetailsService.TryWriteAsync(problemDetailsContext);
            return isHandled;

            //// fallback
            //if(!isHadled)
            //{
            //    httpContext.Response.StatusCode = statusCode;
            //    httpContext.Response.ContentType = "application/problem+json";
            //    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            //    return true;
            //}
        }

        private static string[] SplitStackTrace(string? stackTrace)
        {
            if (string.IsNullOrWhiteSpace(stackTrace))
                return [];

            return stackTrace
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .ToArray();
        }
    }    
}
