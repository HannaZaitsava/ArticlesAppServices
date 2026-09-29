using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Abstractions.IntegrationEvents;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.Abstractions;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using ArticlesService.Infrastructure.DataAccess.DbContext.Interceptors;
using ArticlesService.Infrastructure.DataAccess.Repositories;
using ArticlesService.Infrastructure.DataAccess.Repositories.ConcreteRepositories;
using ArticlesService.Infrastructure.DataAccess.Services;
using ArticlesService.Infrastructure.DataAccess.Settings;
using ArticlesService.Infrastructure.DataAccess.UOW;
using BuildingBlocks.Configuration.ConfigurationValidation;
using BuildingBlocks.IntegrationEventLogEF.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ArticlesService.Infrastructure.DataAccess.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccessServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        {
            AddArticlesServiceDatabase(services, configuration, environment);

            AddIdentity(services);

            AddRepositories(services);

            AddIntegrationEventServices(services);

            services.AddScoped<IDbInitializer, DbInitializer>();

            return services;
        }

        public static IServiceCollection AddArticlesServiceDatabase(this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.AddScoped<AuditAndSoftDeleteInterceptor>();
            services.AddScoped<PublishDomainEventsInterceptor>();

            services.AddWithFluentValidation<DatabaseOptions, DatabaseOptionsValidator>(DatabaseOptions.SectionName);

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var dbSettings = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                options.UseNpgsql(dbSettings.BaseDbConnection);

                // порядок регистрации интерсепторов важен
                options.AddInterceptors(
                    sp.GetRequiredService<PublishDomainEventsInterceptor>(),
                    sp.GetRequiredService<AuditAndSoftDeleteInterceptor>());

                //options.AddInterceptors(
                //  sp.GetRequiredService<PublishDomainEventsInterceptor>());

                if (environment.IsProduction())
                    options.EnableSensitiveDataLogging(false);

            }, ServiceLifetime.Scoped);

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }       

        private static void AddIdentity(IServiceCollection services)
        {
            // Настройка хранения ключей шифрования в БД
            services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();

            services.AddIdentityCore<User>(opt =>
            {
                opt.Password.RequiredLength = 7;
                opt.Password.RequireDigit = false;
                opt.Password.RequireUppercase = false;
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
            })
               .AddRoles<IdentityRole<Guid>>()
               .AddRoleManager<RoleManager<IdentityRole<Guid>>>()
               .AddRoleValidator<RoleValidator<IdentityRole<Guid>>>()
               .AddEntityFrameworkStores<AppDbContext>()
               .AddDefaultTokenProviders(); // this method requires the IDataProtectionProvider

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                //opt.TokenLifespan = TimeSpan.FromDays(365 * 100)); // условно бесконечный токен (на 100 лет) - для тестирования
                opt.TokenLifespan = TimeSpan.FromHours(2));
        }

        private static void AddRepositories(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>))
                .AddScoped<IArticleRepository, ArticleRepository>()
                .AddScoped<IArticleCategoryRepository, ArticleCategoryRepository>()
                .AddScoped<ICommentRepository, CommentRepository>()
                .AddScoped<ITagRepository, TagRepository>();
                //.AddScoped<IUserRepository, UserRepository>();
        }

        private static void AddIntegrationEventServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IIntegrationEventLogService, IntegrationEventLogService<AppDbContext>>();
            serviceCollection.AddScoped<IIntegrationEventService, IntegrationEventService>();
        }
    }
}
