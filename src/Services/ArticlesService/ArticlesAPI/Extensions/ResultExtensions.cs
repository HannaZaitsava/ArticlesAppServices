using ArticlesService.Domain.Enums;
using ArticlesService.Domain.Errors;
using Microsoft.AspNetCore.Mvc;

namespace ArticlesService.ArticlesAPI.Extensions
{
    public static class ResultExtensions
    {
        public static ActionResult ToProblem(this List<Error> errors, HttpContext httpContext)
        {
            if (errors == null || errors.Count == 0)
            {
                return new ObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred"
                });
            }

            return CreateProblem(errors, httpContext);
        }

        private static ActionResult CreateProblem(List<Error> errors, HttpContext httpContext)
        {
            var firstError = errors.FirstOrDefault() ?? new Error("Error", "Unknown error");
            
            if(firstError.Type is ErrorType.Validation)            
            {
                return new BadRequestObjectResult(EnrichProblem(new ValidationProblemDetails(
                    errors
                       .Where(f => f != null)
                       .GroupBy(x => x.Name, x => x.Message)
                       .ToDictionary(g => g.Key, g => g.ToArray()))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred."
                }, httpContext));
            }
            else
            {
                var problemDetails = EnrichProblem(CreateProblemDetails(firstError), httpContext);

                return firstError.Type switch
                {
                    ErrorType.Failure => new BadRequestObjectResult(CreateProblemDetails(firstError)),
                    ErrorType.Validation => new BadRequestObjectResult(CreateProblemDetails(firstError)),
                    ErrorType.NotFound => new NotFoundObjectResult(CreateProblemDetails(firstError)),
                    ErrorType.AlreadyExist => new ConflictObjectResult(CreateProblemDetails(firstError)),
                    ErrorType.Conflict => new ConflictObjectResult(CreateProblemDetails(firstError)),
                    ErrorType.Unauthorized => new UnauthorizedResult(),
                    ErrorType.Forbidden => new ForbidResult(),
                    ErrorType.BadRequest => new BadRequestObjectResult(CreateProblemDetails(firstError)),
                    _ => new ObjectResult(EnrichProblem(CreateProblemDetails(firstError), httpContext))
                    {
                        StatusCode = StatusCodes.Status500InternalServerError
                    }
                };
            }
        }
       
        private static ProblemDetails CreateProblemDetails(Error error) => new()
        {
            Title = error.Name,
            Detail = error.Message
        };

        private static ProblemDetails EnrichProblem(ProblemDetails problemDetails, HttpContext context)
        {
            problemDetails.Instance = $"{context.Request.Method} {context.Request.Path}";
            problemDetails.Extensions.TryAdd("server", Environment.MachineName);
            problemDetails.Extensions.TryAdd("traceId", context.TraceIdentifier);
            return problemDetails;
        }        
    }
}
