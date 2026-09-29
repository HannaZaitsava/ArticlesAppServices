using FluentValidation;
using NotificationService.Settings;

namespace NotificationService.Validators
{
    public class CacheOptionsValidator : AbstractValidator<CacheOptions>
    {
        public CacheOptionsValidator()
        {
            RuleFor(x => x.RedisUrl)
                .NotEmpty()
                .WithMessage("Redis 'RedisUrl' is mandatory in configuration.");

            RuleFor(x => x.InstanceName)
                .NotEmpty()
                .WithMessage("Redis 'InstanceName' is required.");

            RuleFor(x => x.InstanceName)
                .Must(name => name.EndsWith(":"))
                .WithMessage("Redis 'InstanceName' must end with ':' for proper namespacing.");

            RuleFor(x => x.IdempotencyTtlSeconds)
            .GreaterThan(0)
            .WithMessage("Redis 'IdempotencyTtlSeconds' must be greater than 0.");
        }
    }
}
