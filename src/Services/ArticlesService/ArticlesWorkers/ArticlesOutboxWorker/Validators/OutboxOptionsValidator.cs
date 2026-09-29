using ArticlesOutboxWorker.Settings;
using FluentValidation;

namespace ArticlesOutboxWorker.Validators;

public class OutboxOptionsValidator : AbstractValidator<OutboxOptions>
{
    public OutboxOptionsValidator()
    {
        RuleFor(x => x.ActiveIntervalMs)
            .GreaterThan(0)
            .WithMessage("Active interval must be greater than 0 milliseconds.")
            .LessThanOrEqualTo(x => x.IdleIntervalMs)
            .WithMessage("Active interval cannot be greater than idle interval.");

        RuleFor(x => x.IdleIntervalMs)
            .GreaterThan(0)
            .WithMessage("Idle interval must be greater than 0 milliseconds.")
            .LessThanOrEqualTo(x => x.ErrorIntervalMs)
            .WithMessage("Idle interval should typically be less than or equal to error interval.");

        RuleFor(x => x.ErrorIntervalMs)
            .GreaterThan(0)
            .WithMessage("Error interval must be greater than 0 milliseconds.")
            .GreaterThanOrEqualTo(1000)
            .WithMessage("Error interval should be at least 1000 milliseconds to prevent database flooding during outages.");

        RuleFor(x => x.BatchSize)
            .InclusiveBetween(1, 1000)
            .WithMessage("Batch size must be between 1 and 1000 items.");

        RuleFor(x => x.InProgressTimeoutMs)
            .GreaterThanOrEqualTo(10000) // Например, минимум 10 секунд, чтобы не сбросить живой батч
            .WithMessage("InProgress timeout must be at least 10000 milliseconds (10 seconds).");

        RuleFor(x => x.MaxTimesSent)
           .InclusiveBetween(1, 10)
           .WithMessage("Maximum times sent must be between 1 and 10 items.");
    }
}