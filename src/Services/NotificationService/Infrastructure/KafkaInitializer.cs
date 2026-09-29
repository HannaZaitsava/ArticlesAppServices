using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using NotificationService.Settings;

namespace NotificationService.Infrastructure;

public class KafkaInitializer(
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<KafkaInitializer> logger)
{
    private readonly KafkaOptions _options = kafkaOptions.Value;
   
    public async Task EnsureTopicExistsAsync()
    {
        var config = new AdminClientConfig
        {
            BootstrapServers = _options.Consumer.BootstrapServers
        };

        using var adminClient = new AdminClientBuilder(config).Build();

        try
        {
            logger.LogInformation("Consumer thread is checking for the existence of topic '{Topic}'...", _options.TopicName);

            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            // Список топиков, которые приложение должно гарантированно создать
            var topicsToCreate = new List<TopicSpecification>();

            // 1. Проверяем основной топик
            if (!metadata.Topics.Any(t => t.Topic == _options.TopicName))
            {
                logger.LogInformation("Topic '{Topic}' not found. Preparing to create with {Partitions} partitions...",
                    _options.TopicName, _options.NumberOfPartitions);

                topicsToCreate.Add(new TopicSpecification
                {
                    Name = _options.TopicName,
                    NumPartitions = _options.NumberOfPartitions,
                    ReplicationFactor = _options.ReplicationFactor
                });
            }

            // 2. Проверяем DLQ топик
            if (!metadata.Topics.Any(t => t.Topic == _options.DlqTopicName))
            {
                logger.LogInformation("DLQ Topic '{Topic}' not found. Preparing to create...", _options.DlqTopicName);

                topicsToCreate.Add(new TopicSpecification
                {
                    Name = _options.DlqTopicName,
                    NumPartitions = 1, // Для DLQ 1 партиции обычно более чем достаточно, так как там нет высокой нагрузки
                    ReplicationFactor = _options.ReplicationFactor
                });
            }

            // Если каких-то топиков не хватает — создаем пачкой за один сетевой запрос
            if (topicsToCreate.Count > 0)
            {
                await adminClient.CreateTopicsAsync(topicsToCreate);
                logger.LogInformation("All missing Kafka topics were successfully prepared!");
            }
            else
            {
                logger.LogInformation("All required Kafka topics are already operational.");
            }
        }
        catch (CreateTopicsException e)
        {
            // Разделяем результаты: какие топики уже были, а какие упали по другой причине
            var alreadyExistsTopics = e.Results
                .Where(r => r.Error.Code == ErrorCode.TopicAlreadyExists)
                .Select(r => r.Topic)
                .ToList();

            var criticalErrors = e.Results
                .Where(r => r.Error.Code != ErrorCode.NoError && r.Error.Code != ErrorCode.TopicAlreadyExists)
                .ToList();

            if (alreadyExistsTopics.Count > 0)
            {
                logger.LogInformation("The following topics were already created by a concurrent process: {Topics}",
                    string.Join(", ", alreadyExistsTopics));
            }

            // Если помимо "TopicAlreadyExists" произошла реальная ошибка с другим топиком — роняем сервис
            if (criticalErrors.Count > 0)
            {
                foreach (var error in criticalErrors)
                {
                    logger.LogError("Failed to create topic '{Topic}' due to: {Reason} (Code: {Code})",
                        error.Topic, error.Error.Reason, error.Error.Code);
                }
                throw; // Fail Fast
            }
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Critical error: Consumer failed to prepare Kafka topic.");
            throw; // Fail Fast: crash the application immediately if the broker is unreachable
        }
    }
}
