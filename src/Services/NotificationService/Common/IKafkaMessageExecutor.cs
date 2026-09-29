using Confluent.Kafka;

namespace NotificationService.Common
{
    public interface IKafkaMessageExecutor
    {
        Task ExecuteAsync<TKey, TValue>(
            ConsumeResult<TKey, TValue> consumeResult,
            string activityName,
            Func<ConsumeResult<TKey, TValue>, Task> businessLogic);
    }
}
