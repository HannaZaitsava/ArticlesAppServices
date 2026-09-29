using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Confluent.Kafka;

namespace NotificationService.Common
{
    public class KafkaMessageExecutor : IKafkaMessageExecutor
    {
        private readonly ILogger<KafkaMessageExecutor> _logger;

        public KafkaMessageExecutor(ILogger<KafkaMessageExecutor> logger)
        {
            _logger = logger;
        }

        public async Task ExecuteAsync<TKey, TValue>(
            ConsumeResult<TKey, TValue> consumeResult,
            string activityName,
            Func<ConsumeResult<TKey, TValue>, Task> businessLogic)
        {
            // 1. Извлекаем traceparent из заголовков Kafka
            string? traceParentId = null;
            if (consumeResult.Message.Headers.TryGetLastBytes("traceparent", out var headerBytes))
            {
                traceParentId = Encoding.UTF8.GetString(headerBytes);
            }

            // 2. Инициализируем Activity (TraceId)
            using var activity = new Activity(activityName);
            if (!string.IsNullOrEmpty(traceParentId))
            {
                activity.SetParentId(traceParentId);
            }

            activity.Start();

            // Добавляем метаданные в логи по умолчанию для удобного поиска в Seq
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["KafkaTopic"] = consumeResult.Topic,
                ["KafkaPartition"] = consumeResult.Partition.Value,
                ["KafkaOffset"] = consumeResult.Offset.Value
            });

            _logger.LogInformation("Start processing Kafka message from topic {Topic}.", consumeResult.Topic);

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // 3. Выполняем переданную бизнес-логику (наш делегат)
                await businessLogic(consumeResult);

                stopwatch.Stop();
                _logger.LogInformation("Successfully processed Kafka message. Elapsed: {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to process Kafka message after {ElapsedMs}ms.", stopwatch.ElapsedMilliseconds);

                // В продакшене тут часто логику отправки в Dead Letter Queue (DLQ) делают, 
                // либо прокидывают ошибку дальше, чтобы сделать Retry.
                throw;
            }
            finally
            {
                activity.Stop();
            }
        }
    }
}
