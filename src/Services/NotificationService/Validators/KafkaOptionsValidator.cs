using Confluent.Kafka;
using FluentValidation;
using NotificationService.Settings;

namespace NotificationService.Validators;

public class KafkaOptionsValidator : AbstractValidator<KafkaOptions>
{
    public KafkaOptionsValidator(IHostEnvironment environment)
    {
        RuleFor(x => x.TopicName)
            .NotEmpty()
            .WithMessage("Kafka 'TopicName' is mandatory and cannot be empty.");

        RuleFor(x => x.DlqTopicName)
           .NotEmpty()
           .WithMessage("Kafka 'DlqTopicName' is mandatory for Dead Letter Queue routing.");


        RuleFor(x => x.Consumer.BootstrapServers)
            .NotEmpty()
            .WithMessage("Kafka 'BootstrapServers' address is required in configuration.");

        RuleFor(x => x.Producer.BootstrapServers)
          .NotEmpty()
          .WithMessage("Kafka Producer 'BootstrapServers' address is required.");


        RuleFor(x => x.Consumer.GroupId)
            .NotEmpty()
            .WithMessage("Kafka 'GroupId' is required for the consumer to start.");

        RuleFor(x => x.Consumer.EnableAutoCommit)
          .Equal(true)
          .WithMessage("Kafka 'EnableAutoCommit' must be set to true.");

        RuleFor(x => x.Consumer.SessionTimeoutMs)
            .LessThan(x => x.Consumer.MaxPollIntervalMs)
            .WithMessage("Kafka 'SessionTimeoutMs' must be less than 'MaxPollIntervalMs' to prevent rebalance loops.");

        RuleFor(x => x.Consumer.QueuedMinMessages)
           .LessThanOrEqualTo(1000)
           .WithMessage("Kafka 'QueuedMinMessages' should be 1000 or less in production to prevent high memory consumption.");
               
        if (environment.IsDevelopment())
        {
            RuleFor(x => x.ReplicationFactor)
                .Equal((short)1)
                .WithMessage("In Development environment, Kafka 'ReplicationFactor' must be 1.");
        }
        else
        {
            RuleFor(x => x.ReplicationFactor)
                .Equal((short)3)
                .WithMessage("In Production environment, Kafka 'ReplicationFactor' must be 3 for high availability.");

            RuleFor(x => x.Producer.Acks)
                .Equal(Acks.All)
                .WithMessage("In Production, Kafka Producer 'Acks' must be 'All' to guarantee DLQ delivery reliability.");

            RuleFor(x => x.Producer.EnableIdempotence)
                .Equal(true)
                .WithMessage("In Production, Kafka Producer 'EnableIdempotence' must be true to prevent duplicate messages in DLQ during retries.");
        }
       
        RuleFor(x => x.NumberOfPartitions)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Kafka 'NumberOfPartitions' must be at least 1.");

        if (!environment.IsDevelopment())
        {
            RuleFor(x => x.NumberOfPartitions)
                .GreaterThanOrEqualTo(3)
                .WithMessage("In Production, Kafka 'NumPartitions' should be at least 3 for performance scalability.");
        }
    }
}
