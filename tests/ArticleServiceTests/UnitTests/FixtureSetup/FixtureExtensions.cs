using ArticlesService.Tests.UnitTests.Common;
using AutoFixture;
using MapsterMapper;

namespace ArticlesService.Tests.UnitTests.FixtureSetup
{
    public static class FixtureExtensions
    {
        public static IFixture UseMapster(this IFixture fixture)
        {
            var mapper = new Mapper(TestMappingConfig.Instance);
            fixture.Inject<IMapper>(mapper);
            return fixture;
        }
    }
}
