using Confluent.Kafka;

namespace NotificationService.Settings;

public record KafkaOptions
{
    public const string SectionName = "Kafka";

    public string TopicName { get; init; } = string.Empty;
    public string DlqTopicName { get; init; } = string.Empty;

    // дефолтное значение для Production (3)
    public int NumberOfPartitions { get; init; } = 3;
    public short ReplicationFactor { get; init; } = 3;

    public ConsumerConfig Consumer { get; init; } = new();
    public ProducerConfig Producer { get; init; } = new();
}
