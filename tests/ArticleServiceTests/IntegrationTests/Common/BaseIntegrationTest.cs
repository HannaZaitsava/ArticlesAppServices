using ArticlesService.Tests.IntegrationTests.FixtureCustomizations;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;

namespace ArticlesService.Tests.IntegrationTests.Common
{
    public abstract class BaseIntegrationTest : IClassFixture<ArticlesAppFactory>
    {
        protected readonly IServiceScope Scope;
        protected readonly HttpClient Client;
        protected readonly AppDbContext DbContext;
        protected IFixture Fixture;

        protected BaseIntegrationTest(ArticlesAppFactory factory)
        {            
            Client = factory.CreateClient();
            Scope = factory.Services.CreateScope();
            DbContext = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Fixture = CreateFixture();
        }

        /// <summary>
        /// Метод для очистки кэша ChangeTracker.
        /// </summary>
        /// <remarks>
        /// Применяется после секции Arrange
        /// </remarks>
        protected void ClearTracker() => DbContext.ChangeTracker.Clear();

        protected virtual IFixture CreateFixture() => new Fixture().Customize(new ArticlesAppCompositeCustomization());
    }
}
