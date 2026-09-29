using System.Reflection;
using ArticlesService.Domain.Enums;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using FluentValidation;
using MediatR;

namespace ArticlesService.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
     : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!validators.Any())
            {
                return await next(cancellationToken);
            }

            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));                    

            var errors = validationResults
               .SelectMany(r => r.Errors)
               .Where(f => f != null)
               .Select(f => new Error(f.PropertyName, f.ErrorMessage, ErrorType.Validation))
               .ToList();
            
            if (errors.Any())
            {
                return CreateValidationResult<TResponse>(errors);
            }

            return await next(cancellationToken);
        }
        
        private static TResponse CreateValidationResult<T>(List<Error> errors)
        {
            var responseType = typeof(TResponse);

            if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = responseType.GetMethod(
                   "Failure",
                   BindingFlags.Public | BindingFlags.Static,
                   [typeof(List<Error>)]);

                if (failureMethod is null)
                {
                    throw new InvalidOperationException($"Method 'Failure(List<Error>)' was not found in type '{responseType.Name}'.");
                }

                return (TResponse)failureMethod.Invoke(null, [errors])!;
            }

            // Если не Result<T>, выбрасываем исключение
            var errorMessage = string.Join(Environment.NewLine, errors.Select(e => e.Message));
            throw new ValidationException($"Validation failed: {Environment.NewLine}{errorMessage}");
        }
    }
}
