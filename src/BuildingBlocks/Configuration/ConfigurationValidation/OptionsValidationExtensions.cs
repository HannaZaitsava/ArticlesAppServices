using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Configuration.ConfigurationValidation
{
    public static class OptionsValidationExtensions
    {
        public static OptionsBuilder<TOptions> AddWithFluentValidation<TOptions, TValidator>(
            this IServiceCollection services,
            string sectionName)
            where TOptions : class
            where TValidator : class, IValidator<TOptions>
        {
            services.AddTransient<IValidator<TOptions>, TValidator>();

            return services.AddOptions<TOptions>()            
                .BindConfiguration(sectionName, options =>
                {
                    // строгая проверка структуры JSON 
                    options.ErrorOnUnknownConfiguration = true; 
                })
                .ValidateWithFluentValidation()
                .ValidateOnStart();
        }

        private static OptionsBuilder<TOptions> ValidateWithFluentValidation<TOptions>(
            this OptionsBuilder<TOptions> optionsBuilder) 
            where TOptions : class
        {            
            optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>>(
                sp => new FluentValidateOptions<TOptions>(optionsBuilder.Name, sp));

            return optionsBuilder;
        }

        /// <summary>
        /// Извлекает секцию конфигурации, немедленно валидирует её через FluentValidation 
        /// и возвращает готовый объект настроек. Реализует паттерн Fail-Fast.
        /// </summary>
        /// <remarks>
        /// Необходим, когда нужно получить строку конфигурации, а не передать делегат для настройки
        /// </remarks>
        public static TOptions GetValidatedOptions<TOptions, TValidator>(
            this IConfiguration configuration,
            string sectionName)
            where TOptions : class
            where TValidator : class, IValidator<TOptions>, new()
        {
            var section = configuration.GetSection(sectionName);
            var options = section.Get<TOptions>();

            if (options is null)
            {               
                throw new InvalidOperationException(
                    $"Critical startup error: The configuration section '{sectionName}' is completely missing from the appsettings files.");
            }

            var validator = new TValidator();
            var validationResult = validator.Validate(options);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);

                throw new OptionsValidationException(
                    sectionName,
                    typeof(TOptions),
                    errors);
            }

            return options;
        }
    }
}
