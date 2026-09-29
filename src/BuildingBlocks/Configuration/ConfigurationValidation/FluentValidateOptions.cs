using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Configuration.ConfigurationValidation
{
    public class FluentValidateOptions<TOptions>(
        string? name,
        IServiceProvider serviceProvider)
        : IValidateOptions<TOptions> 
        where TOptions : class
    {
        public ValidateOptionsResult Validate(string? optionsName, TOptions options)
        {
            // Поддерживаем именованные опции (если нужно)
            if (name != null && name != optionsName) 
                return ValidateOptionsResult.Skip;

            using var scope = serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IValidator<TOptions>>();

            var result = validator.Validate(options);

            if (result.IsValid) 
                return ValidateOptionsResult.Success;

            var errors = result.Errors.Select(e =>
                $"Options validation failed for {typeof(TOptions).Name}.{e.PropertyName}: {e.ErrorMessage}");

            return ValidateOptionsResult.Fail(errors);
        }
    }
}
