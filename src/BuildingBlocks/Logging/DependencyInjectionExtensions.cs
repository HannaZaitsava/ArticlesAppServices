using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Settings.Configuration;

namespace BuildingBlocks.Logging
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddSharedLogging(this IServiceCollection services)
        {
            //Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"SERILOG INTERNAL ERROR: {msg}"));

            services.AddSerilog((serviceProvider, loggerConfiguration) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                // Используем GetService, так как в некоторых тестах хоста может не быть
                var environment = serviceProvider.GetService<IHostEnvironment>(); 

                var environmentName = environment?.EnvironmentName ?? "Production";
                var applicationName = environment?.ApplicationName ?? "UnknownService";
                var version = GetApplicationVersion();

                loggerConfiguration
                    // Автоматически находит NuGet-пакеты синков в проекте (блок "Using" в JSON больше не нужен)
                    .ReadFrom.Configuration(
                        configuration,
                        new ConfigurationReaderOptions(DependencyContext.Default))

                    // Читаем службы из DI для сложных Enrichers
                    .ReadFrom.Services(serviceProvider)

                    .Enrich.FromLogContext()
                    .Enrich.WithSpan()         // Автоматически подхватит Activity (TraceId и SpanId)
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Application", applicationName)
                    .Enrich.WithProperty("Environment", environmentName)
                    .Enrich.WithProperty("Version", version)
                    .Enrich.FromLogContext()

                     .WriteTo.Seq(
                        serverUrl: configuration["Logging:Seq:ServerUrl"] ?? "http://localhost:5341",
                        apiKey: configuration["Logging:Seq:ApiKey"]) 

                    //.WriteTo.Console(new CompactJsonFormatter())
                    .WriteTo.Conditional(
                        _ => environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase),
                        wt => wt.Console(
                            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
                           // theme: Serilog.Sinks.Console.Themes.AnsiConsoleTheme.Code
                        )
                    )
                    // Для всех остальных сред оставляем компактный JSON
                    .WriteTo.Conditional(
                        _ => !environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase),
                        wt => wt.Console(new CompactJsonFormatter())
                    )                               
                    .Filter.ByExcluding(logEvent =>
                        logEvent.Level == LogEventLevel.Information &&
                        (logEvent.Properties.TryGetValue("RequestPath", out var pathValue) && pathValue.ToString().Contains("/health")
                        || logEvent.Properties.TryGetValue("Uri", out var uri) && uri.ToString().Contains("/health")));
            });

            return services;
        }

        private static string GetApplicationVersion()
        {
            var version = Environment.GetEnvironmentVariable("APP_VERSION");

            if (string.IsNullOrEmpty(version))
            {
                version = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion
                    ?? Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
                    ?? "1.0.0";
            }

            if (version.Contains('+'))
            {
                version = version.Split('+')[0];
            }

            return version;
        }
    }
}
