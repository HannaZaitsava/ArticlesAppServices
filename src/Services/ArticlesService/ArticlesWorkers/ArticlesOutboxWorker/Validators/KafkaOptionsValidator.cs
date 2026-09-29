using ArticlesOutboxWorker.Settings;
using FluentValidation;

namespace ArticlesOutboxWorker.Validators
{
    public class KafkaOptionsValidator : AbstractValidator<KafkaOptions>
    {
        public KafkaOptionsValidator()
        {
            RuleFor(x => x.TopicName)
                .NotEmpty()
                .WithMessage("Kafka topic name must not be empty.");

            RuleFor(x => x.Producer)
                .NotNull()
                .WithMessage("Kafka producer settings section (Producer) must not be null.");

            RuleFor(x => x.Producer.BootstrapServers)
                .NotEmpty()
                .WithMessage("Kafka bootstrap servers address (BootstrapServers) must be specified.");
        }
    }
}
