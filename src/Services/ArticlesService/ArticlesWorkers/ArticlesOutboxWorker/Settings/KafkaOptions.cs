using Confluent.Kafka;

namespace ArticlesOutboxWorker.Settings
{
    public record KafkaOptions
    {
        public const string SectionName = "Kafka";
        public string TopicName { get; init; } = string.Empty;                
        public ProducerConfig Producer { get; init; } = new();
    }
}