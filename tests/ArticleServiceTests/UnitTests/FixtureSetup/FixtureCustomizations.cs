using ArticlesService.Application.Common.Caching;
using AutoFixture;

namespace ArticlesService.Tests.UnitTests.FixtureSetup
{   
    public class FixtureCustomizations : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register<ICacheInvalidationContext>(() => new CacheInvalidationContext());
        }
    }
}
