using ArticlesOutboxWorker.KafkaServices;
using ArticlesOutboxWorker.Settings;
using BuildingBlocks.IntegrationEventLogEF.Services;
using Microsoft.Extensions.Options;

namespace ArticlesOutboxWorker.BackgroundServices;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxOptions _outboxOptions;

    public OutboxProcessor(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessor> logger,
        IOptions<OutboxOptions> outboxOptions)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _outboxOptions = outboxOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Outbox publisher background service has been successfully started.");

        try
        {
            // Однократный сброс зависших задач при СТАРТЕ приложения
            // Гарантируем, что если сервис упал, то при первом же его включении все незавершенные
            // (загруженные в батч со статусом InProcess) прошлым процессом сообщения сразу же вернутся в очередь на отправку
            using (var scope = _serviceProvider.CreateScope())
            {
                var eventLogService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();

                _logger.LogInformation("Checking for stale outbox messages in 'InProgress' state...");

                var timeout = TimeSpan.FromMilliseconds(_outboxOptions.InProgressTimeoutMs);

                // Если сообщение висит в процессе дольше заданного времени — сбрасываем его
                await eventLogService.ResetStaleInProgressEventsAsync(timeout);
            }
        }
        catch (Exception ex)
        {
            // Ошибка тут не должна ломать запуск самого воркера, просто логируем её
            _logger.LogError(ex, "Failed to reset stale InProgress outbox messages during startup.");
        }


        while (!ct.IsCancellationRequested)
        {
            var currentDelay = _outboxOptions.IdleIntervalMs;

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var eventLogService = scope.ServiceProvider.GetRequiredService<IIntegrationEventLogService>();
                    var kafkaService = scope.ServiceProvider.GetRequiredService<IKafkaService>();

                    // Атомарно извлекаем батч и сразу блокируем его в БД статусом InProgress
                    var messages = (await eventLogService.RetrieveAndLockEventLogsAsync(_outboxOptions.BatchSize, _outboxOptions.MaxTimesSent, ct)).ToList();

                    if (messages.Count > 0)
                    {
                        _logger.LogInformation("Found and locked {Count} pending outbox messages to publish to Kafka.", messages.Count);

                        // Публикуем сообщения в Kafka и разделяем их на успешные и упавшие
                        var (successIds, failedIds) = await kafkaService.PublishBatchToKafkaAsync(messages, ct);

                        // Пакетно фиксируем финальные статусы 
                        if (successIds.Count > 0)
                        {
                            await eventLogService.BatchMarkEventAsPublishedAsync(successIds);
                        }

                        if (failedIds.Count > 0)
                        {
                            await eventLogService.BatchMarkEventAsFailedAsync(failedIds, _outboxOptions.MaxTimesSent);
                        }

                        _logger.LogInformation("Batch processing completed. Success: {SuccessCount}, Failed: {FailedCount}.", successIds.Count, failedIds.Count);

                        // Раз в очереди были сообщения, то переключаемся на работу с активным интервалом
                        currentDelay = _outboxOptions.ActiveIntervalMs;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A critical error occurred while processing outbox messages batch.");
                currentDelay = _outboxOptions.ErrorIntervalMs;
            }

            await Task.Delay(currentDelay, ct);
        }
    }
}