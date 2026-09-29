using BuildingBlocks.Configuration.ConfigurationValidation;
using NotificationService.Infrastructure;
using NotificationService.Settings;
using NotificationService.Validators;
using NotificationService.Workers;

namespace NotificationService.Extensions
{    
    public static class KafkaExtensions
    {
        public static IServiceCollection AddKafka(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddWithFluentValidation<KafkaOptions, KafkaOptionsValidator>(KafkaOptions.SectionName);

            services.AddTransient<KafkaInitializer>();
            services.AddHostedService<KafkaConsumerWorker>();

            return services;
        }
    }
}
