using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace NotificationService.Extensions
{
    public static class ResiliencePipelineExtensions
    {
        public static IServiceCollection AddResiliencePipeline(this IServiceCollection services)
        {
            return services.AddResiliencePipeline("kafka-consumer-pipeline", pipelineBuilder =>
            {
                pipelineBuilder.AddRetry(new RetryStrategyOptions
                {
                    // СТРАТЕГИЯ 1: Retry (Внешнее кольцо)
                    // Она охватывает весь процесс. Если внутри (в БД или таймауте) произойдет сбой,
                    // Retry перехватит его и запустит всю цепочку заново через паузу.
                    ShouldHandle = new PredicateBuilder()
                     .Handle<KafkaException>()
                    .Handle<DbUpdateException>()     // Сбои базы данных
                    .Handle<HttpRequestException>()  // Сбои внешних API
                    .Handle<TimeoutRejectedException>() // ВАЖНО: перехватываем таймаут от стратегии ниже (AddTimeout)
                    .Handle<TimeoutException>(),     // Обычные таймауты


                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(2),
                    UseJitter = true
                });

                // СТРАТЕГИЯ 2: Timeout (Внутреннее кольцо)
                // Она следит за ОДНОЙ конкретной попыткой выполнения MediatR.
                // Если MediatR "завис" в базе данных дольше чем на 5 секунд, эта стратегия
                // принудительно отменит `pipelineToken`, что вызовет TimeoutRejectedException.
                pipelineBuilder.AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(5)
                });
            });
        }
    }
}
