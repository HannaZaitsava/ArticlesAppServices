using ArticlesApp.Infrastructure.DataAccess.DbContext;
using ArticlesApp.Tests.Shared.FixtureCustomizations;
using AutoFixture;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StackExchange.Redis;

namespace IntegrationTests.Common
{
    [Collection(TestCollections.IntegrationTest)]
    public abstract class BaseCollectionIntegrationTest : IAsyncLifetime
    {
        protected readonly HttpClient _httpClient;
        protected readonly ArticlesAppFactory _appFactory;
        protected readonly IServiceScope _serviceScope;
        protected readonly ISender _sender;
        protected readonly IFixture _fixture = new Fixture().Customize(new ArticlesAppCompositeCustomization());
        private readonly NpgsqlConnection _dbConnection = default!;
        private readonly CollectionFixtureSharedTestContext _sharedTestContext;
       

        protected BaseCollectionIntegrationTest(CollectionFixtureSharedTestContext context)
        {
            _sharedTestContext = context;
            _appFactory = context.AppFactory;
            _httpClient = _appFactory.CreateClient();
            _dbConnection = new NpgsqlConnection(context.DbContainer.GetConnectionString());
            _serviceScope = _appFactory.Services.CreateScope();
            _sender = _serviceScope.ServiceProvider.GetRequiredService<ISender>();
        }

        public async Task InitializeAsync()
        {
            //_dbConnection = new NpgsqlConnection(_dbConnectionString);
            //await _dbConnection.OpenAsync();

            //// Прогоняем миграции (только если база пустая)
            //using var migrationScope = Factory.Services.CreateScope();
            //var db = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
            //await db.Database.MigrateAsync();

            //_respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
            //{
            //    DbAdapter = DbAdapter.Postgres,
            //    SchemasToInclude = DatabaseSchemasToInclude,
            //    // Обязательно игнорируем таблицу миграций, иначе Respawn её тоже почистит
            //    TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
            //});

            // Соединение с БД уже есть в контексте
            await _dbConnection.OpenAsync();

            // Очищаем БД и кэш перед каждым тестом           
            await Task.WhenAll(
                ResetDatabaseAsync(),
                FlushRedisAsync()
            );
        }

        public async Task DisposeAsync()
        {
            await _dbConnection.DisposeAsync();
            _serviceScope.Dispose();
        }

        /// <summary>
        /// Метод для подготовки данных
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        protected async Task ExecuteInDbContext(Func<AppDbContext, Task> action)
        {
            /*
             * Для обычного Web API вариант через Services.CreateScope() лучше, потому что:
                    - Консистентность: Вы используете ровно тот же экземпляр DbContext, который получил бы контроллер.
                        Мы гарантированно работаем с БД так же, как это делает хендлер. 
                        Если в DI добавлены какие-то специфичные настройки (например, фильтры ресурсов), они применятся и здесь.
                    - Scoping: В ASP.NET Core DbContext обычно живет в рамках Scope (один на запрос). 
                        Создавая scope в тесте, мы имитируем жизненный цикл одного HTTP-запроса. 
                        Это гарантирует, что ChangeTracker будет чистым для каждой операции Arrange.
             */
            using var scope = _appFactory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await action(context);            
        }

        /// <summary>
        /// Метод очистки базы данных
        /// </summary>
        /// <returns></returns>
        protected async Task ResetDatabaseAsync()
        {
            await _sharedTestContext.Respawner.ResetAsync(_dbConnection);
        }

        /// <summary>
        /// Метод очистки кэша
        /// </summary>
        /// <returns></returns>
        protected async Task FlushRedisAsync()
        {
            var multiplexer = _serviceScope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();
            foreach (var endpoint in multiplexer.GetEndPoints())
            {
                await multiplexer.GetServer(endpoint).FlushDatabaseAsync();
            }
        }
    }
}
