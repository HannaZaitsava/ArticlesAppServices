using ArticlesApp.Infrastructure.DataAccess.DbContext;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace IntegrationTests.Common
{
    public class CollectionFixtureSharedTestContext : IAsyncLifetime
    {       
        public PostgreSqlContainer DbContainer { get; } = new PostgreSqlBuilder()
            .WithImage("postgres:18-alpine")
            .Build();

        public RedisContainer RedisContainer { get; } = new RedisBuilder()
            .WithImage("redis:8-alpine")
            .Build();

        public ArticlesAppFactory AppFactory { get; private set; } = null!;

        public Respawner Respawner { get; private set; } = default!;
        private static readonly string[] DatabaseSchemasToInclude = new[] { "public" };

        public async Task InitializeAsync()
        {
            AppFactory = new ArticlesAppFactory(this);

            await Task.WhenAll(DbContainer.StartAsync(), RedisContainer.StartAsync());

            var connectionString = DbContainer.GetConnectionString();

            // Создаем настройки для существующего AppDbContext
            // Вместо ServiceCollection используем билдер опций напрямую — это быстрее и чище,
            // т.к. не нужно подниамть весь DI-контейнер, чтобы просто сформировать схему БД
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            // Выполняем миграции через "одноразовый" экземпляр контекста
            using (var context = new AppDbContext(optionsBuilder.Options))
            {
                await context.Database.MigrateAsync();
            }

            using var dbConnection = new NpgsqlConnection(DbContainer.GetConnectionString());
            await dbConnection.OpenAsync();            

            Respawner = await Respawner.CreateAsync(dbConnection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = DatabaseSchemasToInclude,
                // Обязательно игнорируем таблицу миграций, иначе Respawn её тоже почистит
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
            });
        }
       
        public async Task DisposeAsync() =>
            await Task.WhenAll(DbContainer.StopAsync(), RedisContainer.StopAsync());
    }
}
