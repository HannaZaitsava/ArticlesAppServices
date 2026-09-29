using ArticlesService.Infrastructure.DataAccess.DbContext;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ArticlesService.Tests.IntegrationTests.Common
{
    public class SharedTestContext : IAsyncLifetime
    {
        // Статика гарантирует, что контейнеры создадутся 1 раз для всей сборки
        private static readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:18-alpine")
            .Build();

        private static readonly RedisContainer _redisContainer = new RedisBuilder()
            .WithImage("redis:8-alpine")
            .Build();

        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static bool _isInitialized;

        public string DbConnectionString => _dbContainer.GetConnectionString();
        public string RedisConnectionString => _redisContainer.GetConnectionString();

        public async Task InitializeAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (!_isInitialized)
                {
                    await Task.WhenAll(_dbContainer.StartAsync(), _redisContainer.StartAsync());

                    await ApplyMigrationsAsync(DbConnectionString);

                    _isInitialized = true;
                }
            }
            finally { _lock.Release(); }
        }

        public Task DisposeAsync() => Task.CompletedTask; // Контейнеры уничтожатся вместе с процессом

        private async Task ApplyMigrationsAsync(string connectionString)
        {            
            // Создаем настройки для существующего AppDbContext
            // Вместо ServiceCollection используем билдер опций напрямую — это быстрее и чище,
            // т.к. не нужно подниамать весь DI-контейнер, чтобы просто сформировать схему БД.
            // Создаем опции для DbContext вручную, так как DI приложения еще не готов.
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(_dbContainer.GetConnectionString());

            using var context = new AppDbContext(optionsBuilder.Options);
           
            await context.Database.MigrateAsync();
        }
    }
}
