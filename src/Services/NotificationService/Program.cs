using BuildingBlocks.Logging;
using NotificationService.CommandIdempotency;
using NotificationService.Extensions;
using NotificationService.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting NotificationService...");

    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSingleton(TimeProvider.System);
    builder.Services.AddSharedLogging();
    builder.Services.AddResiliencePipeline();

    builder.Services.AddMediatR(cfg => {
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        cfg.AddOpenBehavior(typeof(IdempotentBehavior<,>));
    });

    builder.Services.AddKafka(builder.Configuration);
    builder.Services.AddRedisCache(builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<KafkaInitializer>();
        await initializer.EnsureTopicExistsAsync();
    }

    Log.Information("NotificationService host configuration completed successfully.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "NotificationService terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}