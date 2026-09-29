using ArticlesOutboxWorker.BackgroundServices;
using ArticlesOutboxWorker.KafkaServices;
using ArticlesOutboxWorker.Settings;
using ArticlesOutboxWorker.Validators;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using BuildingBlocks.Configuration.ConfigurationValidation;
using BuildingBlocks.IntegrationEventLogEF.Services;
using BuildingBlocks.Logging;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ArticlesOutboxWorker...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddSingleton<IKafkaService, KafkaService>();

    builder.Services.AddSharedLogging();
    
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("BaseDbConnection"),
            npgsqlOptions => 
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null
                );
            })
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // т.к. в коде часто используется ExecuteUpdateAsync()
    });

    builder.Services.AddWithFluentValidation<OutboxOptions, OutboxOptionsValidator>(OutboxOptions.SectionName);
    builder.Services.AddWithFluentValidation<KafkaOptions, KafkaOptionsValidator>(KafkaOptions.SectionName);

    builder.Services.AddHostedService<OutboxProcessor>();       
    builder.Services.AddHostedService<OutboxCleanerProcessor>();
    builder.Services.AddScoped<IIntegrationEventLogService, IntegrationEventLogService<AppDbContext>>();

    var host = builder.Build();
    Log.Information("ArticlesOutboxWorker host configuration completed successfully.");

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ArticlesOutboxWorker terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}