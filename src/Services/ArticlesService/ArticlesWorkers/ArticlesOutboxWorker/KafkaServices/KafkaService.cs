using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using ArticlesOutboxWorker.Settings;
using BuildingBlocks.IntegrationEventLogEF;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace ArticlesOutboxWorker.KafkaServices
{
    public class KafkaService : IKafkaService
    {
        ILogger<KafkaService> _logger;
        private readonly IProducer<string, string> _producer;
        private readonly KafkaOptions _kafkaOptions;        

        public KafkaService(
            ILogger<KafkaService> logger,
            IOptions<KafkaOptions> kafkaOptions)
        {
            _logger = logger;
            _kafkaOptions = kafkaOptions.Value;
            _producer = new ProducerBuilder<string, string>(_kafkaOptions.Producer).Build();
        }
        
        public async Task<(List<Guid> SuccessIds, List<Guid> FailedIds)> PublishBatchToKafkaAsync(
            IEnumerable<IntegrationEventLogEntry> messages,
            CancellationToken ct)
        {
            // Используем потокобезопасные коллекции, так как запись будет идти параллельно
            var successIds = new ConcurrentBag<Guid>();
            var failedIds = new ConcurrentBag<Guid>();

            // Превращаем коллекцию сообщений в коллекцию запущенных задач (Tasks)
            var tasks = messages.Select(async message =>
            {
                if (ct.IsCancellationRequested) return;

                try
                {
                    var headers = new Headers
                    {
                        { "x-event-type", Encoding.UTF8.GetBytes(message.EventTypeShortName) },
                        { "traceparent", Encoding.UTF8.GetBytes(message.TraceParent ?? string.Empty) }
                    };

                    var kafkaMessage = new Message<string, string>
                    {
                        Key = message.EventId.ToString(),
                        Value = message.Content,
                        Headers = headers
                    };

                    // Запускаем асинхронную отправку. librdkafka МГНОВЕННО забирает сообщение 
                    // во внутренний буфер и объединяет его с другими сообщениями из этого Select в один сетевой батч.
                    var result = await _producer.ProduceAsync(_kafkaOptions.TopicName, kafkaMessage, ct);

                    successIds.Add(message.EventId);
                    _logger.LogDebug("Message {Id} (Type: {Type}) successfully published to Kafka.", message.EventId, message.EventTypeShortName);
                }
                catch (ProduceException<string, string> ex)
                {
                    // Ошибка, специфичная для Kafka (например, сообщение слишком большое или брокер недоступен после всех ретраев)
                    _logger.LogError(ex, "Kafka produce exception for message {Id}. Error: {Reason}", message.EventId, ex.Error.Reason);
                    failedIds.Add(message.EventId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while publishing message {Id} to Kafka.", message.EventId);
                    failedIds.Add(message.EventId);
                }
            });

            // Асинхронно ждем, пока завершатся ВСЕ задачи. 
            // Потоки ASP.NET Core свободны. librdkafka в это время отправляет данные огромными пачками.
            await Task.WhenAll(tasks);

            // Конвертируем в List для возвращаемого значения (как и требовал метод)
            return (successIds.ToList(), failedIds.ToList());
        }

        public void Dispose()
        {
            _logger.LogInformation("Flushing and disposing Kafka producer...");

            // Корректно освободить ресурсы Kafka продюсера при остановке приложения.
            // Гарантируем, что все оставшиеся в памяти сообщения отправятся в брокер перед выключением
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
