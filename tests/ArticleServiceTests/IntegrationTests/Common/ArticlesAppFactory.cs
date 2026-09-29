using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;


namespace ArticlesService.Tests.IntegrationTests.Common
{
    //public class ArticlesAppFactory(CollectionFixtureSharedTestContext context) : WebApplicationFactory<Program>
    //{
    //    protected override void ConfigureWebHost(IWebHostBuilder builder)
    //    {
    //        /*
    //         * UseSetting используется для динамической передачи строк подключения. Наше приложение будет использовать их автоматически.
    //         * Это также позволяет избежать состояний гонки или конфликтов с другими тестами, которые могут выполняться параллельно. 
    //         * Это гарантирует, что наши тесты всегда будут подключаться к правильным портам, независимо от того, какие порты назначает Docker. 
    //         * Нет необходимости удалять сервисы из коллекции сервисов или настраивать их вручную. 
    //         * 
    //         * Гонка в тестах — это когда один тест "подворовывает" настройки у другого.
    //         * UseSetting делает конфигурацию потокобезопасной, привязывая её к конкретному экземпляру фабрики, а не к глобальному окружению системы.
    //         */

    //        // Имена строк подключения в UseSetting (ConnectionStrings:Database) должны точно совпадать с теми, что ожидает код в Program.cs.
    //        builder.UseSetting("ConnectionStrings:BaseDbConnection", context.DbContainer.GetConnectionString());
    //        // allowAdmin=true - По умолчанию команда FLUSHDB (которая полностью очищает базу) считается «опасной»,
    //        // и библиотека StackExchange.Redis блокирует её выполнение, если в строке подключения явно не разрешен режим администратора.
    //        builder.UseSetting("CacheSettings:RedisUrl", context.RedisContainer.GetConnectionString() + ",connectTimeout=5000,syncTimeout=5000,abortConnect=false,allowAdmin=true");
    //        // Если для тестов нужно форсировать использование распределенного кэша
    //        builder.UseSetting("CacheSettings:UseDistributedCache", "true");
    //    }
    //}

    /// <summary>
    /// Фабрика подменяет настройки для каждого тестового класса, сохраняя при этом общую базу данных
    /// </summary>
    public class ArticlesAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly SharedTestContext _sharedContext = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Каждому КЛАССУ тестов — свой уникальный префикс в Redis для изоляции
            var testInstancePrefix = $"Test_{Guid.NewGuid():N}:";

            /*
            * UseSetting используется для динамической передачи строк подключения. Наше приложение будет использовать их автоматически.
            * Это также позволяет избежать состояний гонки или конфликтов с другими тестами, которые могут выполняться параллельно. 
            * Это гарантирует, что наши тесты всегда будут подключаться к правильным портам, независимо от того, какие порты назначает Docker. 
            * Нет необходимости удалять сервисы из коллекции сервисов или настраивать их вручную. 
            * 
            * Гонка в тестах — это когда один тест "подворовывает" настройки у другого.
            * UseSetting делает конфигурацию потокобезопасной, привязывая её к конкретному экземпляру фабрики, а не к глобальному окружению системы.
            */

            // Имена строк подключения в UseSetting (ConnectionStrings:Database) должны точно совпадать с теми, что ожидает код в Program.cs.
            builder.UseSetting("ConnectionStrings:BaseDbConnection", _sharedContext.DbConnectionString);
            builder.UseSetting("CacheSettings:RedisUrl", _sharedContext.RedisConnectionString + ",connectTimeout=5000,syncTimeout=5000,abortConnect=false");
            builder.UseSetting("CacheSettings:InstanceName", testInstancePrefix);
            // Если для тестов нужно форсировать использование распределенного кэша
            builder.UseSetting("CacheSettings:UseDistributedCache", "true");
        }

        public async Task InitializeAsync() => await _sharedContext.InitializeAsync();
        public new async Task DisposeAsync() => await base.DisposeAsync();
    }
}
