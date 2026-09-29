using ArticlesService.Infrastructure.DataAccess.DbContext;
using ArticlesService.Infrastructure.DataAccess.Settings;
using BuildingBlocks.Configuration.ConfigurationValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ArticlesOutboxWorker.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDataBase(this IServiceCollection services, IHostEnvironment environment)
        {
            services.AddWithFluentValidation<DatabaseOptions, DatabaseOptionsValidator>(DatabaseOptions.SectionName);

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var dbSettings = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                options.UseNpgsql(dbSettings.BaseDbConnection);

                if (environment.IsProduction())
                {
                    options.EnableSensitiveDataLogging(false);
                }
            });

            return services;
        }
    }
}
