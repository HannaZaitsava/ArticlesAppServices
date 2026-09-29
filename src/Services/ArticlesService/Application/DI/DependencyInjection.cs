using ArticlesService.Application.Common.Behaviors;
using ArticlesService.Application.Common.Caching;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ArticlesService.Application.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {           
            var assembly = typeof(DependencyInjection).Assembly;
            //// добавить конфигурации Mapster слоя Application в общий список конфигураций маппера.
            //// Минус: смешивание конфигов из разных слоев, что может привести Mapster к путанице, если конфиги из разных слоев будут совпадать 
            //config.Scan(assembly);

            // Регистрируем все валидаторы из сборки
            services.AddValidatorsFromAssembly(assembly);

            services.AddScoped<ICacheInvalidationContext, CacheInvalidationContext>();

            services.AddMediatR(cfg =>
            {
                // Регистрация всех хендлеров из этой сборки
                cfg.RegisterServicesFromAssembly(assembly);
                // Регистрация валидации, логирования и т.д.
                cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
                cfg.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(TransactionBehavior<,>));
            });

            return services;
        }

        public static void  AddApplicationMapperConfigurations(this TypeAdapterConfig config)
        {
            var assembly = typeof(DependencyInjection).Assembly;            
            // добавить конфигурации Mapster слоя Application в общий список конфигураций маппера.
            // Минус: смешивание конфигов из разных слоев, что может привести Mapster к путанице, если конфиги из разных слоев будут совпадать 
            config.Scan(assembly);
        }
    }
}
