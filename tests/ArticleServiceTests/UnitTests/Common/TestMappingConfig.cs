using ArticlesService.Application;
using Mapster;

namespace ArticlesService.Tests.UnitTests.Common
{
    public static class TestMappingConfig
    {
        private static readonly Lazy<TypeAdapterConfig> _config = new(() =>
        {
            var config = new TypeAdapterConfig();

            config.Scan(typeof(IApplicationAssemblyMarker).Assembly);

            config.Compile();
            return config;
        });

        public static TypeAdapterConfig Instance => _config.Value;
    }
}
