using ArticlesService.Infrastructure.DataAccess.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArticlesService.Infrastructure.DataAccess.DI.Extensions
{
    public static class DbMigrationExtensions
    {      
        public static async Task MigrateAndSeedDatabase(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            await dbInitializer.MigrateAsync();
        }
    }
}
