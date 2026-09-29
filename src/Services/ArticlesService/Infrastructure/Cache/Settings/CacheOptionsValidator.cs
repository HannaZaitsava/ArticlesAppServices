using FluentValidation;

namespace ArticlesService.Infrastructure.Cache.Settings
{
    public class CacheOptionsValidator : AbstractValidator<CacheOptions>
    {
        public CacheOptionsValidator()
        {
            RuleFor(x => x.ApiVersion)
                .NotEmpty().WithMessage("ApiVersion cannot be empty (e.g., 'v1.0')")
                .Matches(@"^v\d+(\.\d+)?$").WithMessage("ApiVersion must follow format 'v1' or 'v1.1'");

            RuleFor(x => x.RedisUrl)
                .NotEmpty()
                .When(x => x.UseDistributedCache)
                .WithMessage("RedisUrl is required when distributed cache is enabled.");

            RuleFor(x => x.LocalCacheExpirationSeconds)
                .LessThanOrEqualTo(x => x.ExpirationSeconds)
                .WithMessage("L1 (Local) cache expiration cannot be longer than L2 (Global) cache expiration.");

            RuleFor(x => x.ExpirationSeconds).GreaterThan(0);
        }
    }
}
