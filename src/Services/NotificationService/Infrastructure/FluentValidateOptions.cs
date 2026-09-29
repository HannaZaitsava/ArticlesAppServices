using FluentValidation;
using Microsoft.Extensions.Options;

namespace NotificationService.Infrastructure
{
    public class FluentValidationOptions<TOptions>(
    string? name,
    IValidator<TOptions> validator) : IValidateOptions<TOptions> where TOptions : class
    {
        public ValidateOptionsResult Validate(string? optionsName, TOptions options)
        {
            // Null name means it applies to all named options configurations
            if (name is not null && name != optionsName)
            {
                return ValidateOptionsResult.Skip;
            }

            ArgumentNullException.ThrowIfNull(options);

            var validationResult = validator.Validate(options);

            if (validationResult.IsValid)
            {
                return ValidateOptionsResult.Success;
            }

            // Collect all validation errors into a clean, readable string
            var errors = validationResult.Errors
                .Select(e => $"Options validation failed for '{e.PropertyName}': {e.ErrorMessage}");

            return ValidateOptionsResult.Fail(errors);
        }
    }
}
