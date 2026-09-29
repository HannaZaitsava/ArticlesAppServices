using System.Diagnostics;
using System.Text;
using System.Text.Json;
using BuildingBlocks.Contracts.IntegrationEvents;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Options;
using NotificationService.Commands;
using NotificationService.Settings;
using Polly;
using Polly.Registry;

namespace NotificationService.Workers;

public class KafkaConsumerWorker(
    IOptions<KafkaOptions> kafkaOptions,
    IServiceScopeFactory scopeFactory,
    ILogger<KafkaConsumerWorker> logger,
    ResiliencePipelineProvider<string> resiliencePipelineProvider) : BackgroundService
{
    private readonly IConsumer<string, string> _consumer = new ConsumerBuilder<string, string>(kafkaOptions.Value.Consumer).Build();
    private readonly IProducer<string, string> _dlqProducer = new ProducerBuilder<string, string>(kafkaOptions.Value.Producer).Build();

    private readonly ResiliencePipeline _resiliencePipeline = resiliencePipelineProvider.GetPipeline("kafka-consumer-pipeline");

    private readonly string _topicName = kafkaOptions.Value.TopicName;
    private readonly string _dlqTopicName = kafkaOptions.Value.DlqTopicName;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true 
    };   

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await StartConsumerLoop(ct);
    }

    /*
    Как коммитить обработанныее оффсеты?
     - Использование _consumer.Commit(consumeResult) вместо _consumer.Commit()    
        Если вызвать пустой _consumer.Commit(), библиотека закоммитит всю пачку, которую она успела выкачать из Kafka во внутренний буфер 
        во время последнего вызова метода .Consume(). Это может привести к случайному коммиту еще не обработанных сообщений.    
        Передавая конкретный consumeResult, мы даем команду: «Закоммить строго это сообщение (и все, что были до него)». Это гораздо безопаснее.

     - Использовать StoreOffset (Лучшая практика для 95% задач) - т.к. не блокирует отсновной поток приложения.
    
    Сравнение: Commit vs StoreOffset
    
    Критерий              |  consumer.Commit()                                      |   consumer.StoreOffset()
    --------------------------------------------------------------------------------------------------------------------------------------------
    Тип операции          |  Синхронная / Блокирующая сеть                          |   Локальная / Мгновенная память
    Куда пишутся данные   |  Сразу отправляет запрос брокеру Kafka                  |   Записывает офсет во внутренний буфер приложения
    Кто делает отправку   |  Основной поток вашего приложения                       |   Фоновый поток Kafka с заданной периодичностью
    Влияние на скорость   |  Низкая скорость (ждёт ответа от сети на каждом шаге)   |   Максимальная скорость (не блокирует обработку сообщений)
     */
    private async Task StartConsumerLoop(CancellationToken ct)
    {
        _consumer.Subscribe(_topicName);
        logger.LogInformation("NotificationService successfully subscribed to topic: {Topic}", _topicName);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(ct);
                    if (consumeResult is null) continue;

                    // TODO вынести в обертку
                    // __________________________________________
                    // Пытаемся достать заголовок traceparent
                    string? traceParentId = null;
                    if (consumeResult.Message.Headers.TryGetLastBytes("traceparent", out var headerBytes))
                    {
                        traceParentId = Encoding.UTF8.GetString(headerBytes);
                    }

                    // Создаем активность. Имя "ReceiveFromKafka" появится в системах трассировки, если добавите их позже
                    using var activity = new Activity("ReceiveFromKafka");

                    if (!string.IsNullOrEmpty(traceParentId))
                    {
                        // Связываем эту активность с родителем из Воркера
                        activity.SetParentId(traceParentId);
                    }

                    // Запускаем активность ДО написания логов
                    activity.Start();

                    using var loggerScope = logger.BeginScope(new Dictionary<string, object>
                    {
                        ["KafkaTopic"] = consumeResult.Topic,
                        ["KafkaPartition"] = consumeResult.Partition.Value,
                        ["KafkaOffset"] = consumeResult.Offset.Value
                    });
                    // __________________________________________

                    // TODO: возможно, в будущем настраивать для разных типов сообщений отдельные топики в Кафке
                    var eventTypeHeader = consumeResult.Message.Headers.FirstOrDefault(h => h.Key == "x-event-type");
                    if (eventTypeHeader == null)
                    {
                        continue; 
                    }

                    string eventTypeName = Encoding.UTF8.GetString(eventTypeHeader.GetValueBytes());

                    if (!eventTypeName.EndsWith(nameof(ArticlePublishedIntegrationEvent)))
                    {
                        // логируем только на уровне Debug, чтобы не забивать логи в продакшене
                        logger.LogDebug("Skipping unhandled event type: {Type}", eventTypeName);

                        _consumer.StoreOffset(consumeResult);
                        continue; 
                    }
                    //_________________

                    ArticlePublishedIntegrationEvent? articleEvent = null;

                    try
                    {
                        // Используем настройки нечувствительности к регистру
                        articleEvent = JsonSerializer.Deserialize<ArticlePublishedIntegrationEvent>(consumeResult.Message.Value, JsonOptions);
                    }
                    catch (JsonException ex)
                    {
                        logger.LogError(ex, "Poison pill encountered! Failed to deserialize message body at Offset {Offset}",
                            consumeResult.Offset.Value);

                        // Сообщение сломано, но мы его пропускаем, чтобы не заблокировать очередь.
                        // Коммитим/сдвигаем битый оффсет, чтобы следующий poll взял новое сообщение.
                        await SendToDeadLetterQueueAsync(consumeResult, ex, ct);
                        _consumer.StoreOffset(consumeResult);
                        continue;
                    }

                    if (articleEvent is null)
                    {
                        logger.LogWarning("Deserialized message is null at Offset {Offset}", consumeResult.Offset.Value);

                        // Пустые сообщения тоже нужно прокоммитить и пропустить.
                        var ex = new InvalidOperationException("Message payload deserialized to null");
                        await SendToDeadLetterQueueAsync(consumeResult, ex, ct);
                        _consumer.StoreOffset(consumeResult);
                        continue;
                    }

                    logger.LogInformation("""                       
                        Notification received. 
                        New Article:   '{Title}'
                        Article ID:    {Id}
                        Author ID (K): {Author}
                        Published At:  {Date:yyyy-MM-dd HH:mm:ss UTC}
                        ------------------------------------------------------
                        METADATA -> Partition: [{Partition}] | Offset: [{Offset}]
                        ------------------------------------------------------
                        Sending Email / Push notifications...
                        """,
                        articleEvent.Title,
                        articleEvent.ArticleId,
                        articleEvent.AuthorId,
                        articleEvent.PublishedAt,
                        consumeResult.Partition.Value, 
                        consumeResult.Offset.Value);  

                    var command = new SendArticlePublishedNotificationCommand(
                        articleEvent.ArticleId,
                        articleEvent.Title,
                        articleEvent.AuthorId,
                        articleEvent.PublishedAt
                    );

                    using var scope = scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                                       
                    try
                    {
                        await _resiliencePipeline.ExecuteAsync(async pipelineToken =>
                        {
                            await mediator.Send(command, pipelineToken);
                        });

                        // Бизнес-логика отработала успешно
                        // Передаем consumeResult, чтобы закоммитить конкретно этот обработанный оффсет.
                        _consumer.StoreOffset(consumeResult);
                        logger.LogDebug("Message {Id} successfully processed and committed", consumeResult.Message.Key);
                    }
                    catch (Exception bizEx)
                    {
                        logger.LogError(bizEx, "Business logic failed for message at Offset {Offset}. Deciding what to do...", consumeResult.Offset.Value);

                        // КРИТИЧЕСКИЙ ВЫБОР ДЛЯ PRODUCTION:
                        // Вариант А: Если НЕ вызвать здесь Commit/StoreOffset, воркер упадет, поднимется и снова попробует обработать это же сообщение (может зациклиться).
                        // Вариант Б: Отправить это сообщение в Dead Letter Queue (DLQ топик в Кафке) и вызвать _consumer.Commit(consumeResult)/StoreOffset, чтобы идти дальше.

                        // Ошибка в MediatR/БД — отправляем сообщение в DLQ, чтобы не вешать всю очередь
                        await SendToDeadLetterQueueAsync(consumeResult, bizEx, ct);
                        _consumer.StoreOffset(consumeResult);
                    }
                    finally
                    {
                        activity.Stop();
                    }
                }
                catch (ConsumeException e)
                {
                    logger.LogError("Error occurred while consuming from Kafka: {Reason}", e.Error.Reason);
                }                
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Kafka consumption thread has been canceled.");
        }
        finally
        {
            // Корректно закрываем консьюмера, что инициирует ребалансировку на стороне брокера.
            // Метод Close() перед деструктором гарантирует отправку в брокер все
            // сохраненные через StoreOffset() оффсеты, которые фоновый поток еще не успел закоммитить.
            _consumer.Close();
            _dlqProducer.Dispose();
            logger.LogInformation("Kafka consumer connection closed safely.");
        }
    }

    /// <summary>
    /// Вспомогательный метод для обогащения сообщения метаданными об ошибке и отправки в DLQ
    /// </summary>
    private async Task SendToDeadLetterQueueAsync(ConsumeResult<string, string> sourceResult, Exception exception, CancellationToken ct)
    {
        try
        {
            // Копируем существующие заголовки, если они были
            var headers = sourceResult.Message.Headers ?? new Headers();

            // Добавляем технические заголовки с информацией об ошибке (строго в UTF-8)
            headers.Add("x-dead-letter-reason", Encoding.UTF8.GetBytes(exception.Message));
            headers.Add("x-dead-letter-exception", Encoding.UTF8.GetBytes(exception.GetType().FullName ?? "UnknownException"));
            headers.Add("x-dead-letter-stacktrace", Encoding.UTF8.GetBytes(exception.StackTrace ?? string.Empty));
            headers.Add("x-dead-letter-timestamp", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("o")));
            headers.Add("x-dead-letter-service", Encoding.UTF8.GetBytes("NotificationService"));
            headers.Add("x-original-offset", Encoding.UTF8.GetBytes(sourceResult.Offset.Value.ToString()));
            headers.Add("x-original-partition", Encoding.UTF8.GetBytes(sourceResult.Partition.Value.ToString()));
            
            var dlqMessage = new Message<string, string>
            {
                Key = sourceResult.Message.Key,
                Value = sourceResult.Message.Value, // Пересылаем сырое тело «как есть» [1]
                Headers = headers
            };

            // Отправляем в DLQ топик
            await _dlqProducer.ProduceAsync(_dlqTopicName, dlqMessage, ct);

            logger.LogInformation("Message from partition {Partition}, offset {Offset} successfully moved to DLQ: {DlqTopic}",
                sourceResult.Partition.Value, sourceResult.Offset.Value, _dlqTopicName);
        }
        catch (Exception dlqEx)
        {
            // КРИТИЧЕСКАЯ СИТУАЦИЯ: Сам DLQ топик недоступен. 
            // TODO: В проде здесь лучше настроить отправку критического алерта в систему мониторинга
            logger.LogCritical(dlqEx, "FATAL: Failed to send message to DLQ. System might stall to prevent data loss. Original Offset: {Offset}",
                sourceResult.Offset.Value);

            // Пробрасываем ошибку наружу, чтобы остановить воркер и не потерять данные скрытно
            throw;
        }
    }
}
