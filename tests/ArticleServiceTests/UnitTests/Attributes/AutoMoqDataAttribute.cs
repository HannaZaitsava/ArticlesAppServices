using ArticlesService.Tests.Shared.FixtureCustomizations;
using ArticlesService.Tests.UnitTests.FixtureSetup;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace ArticlesService.Tests.UnitTests.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute() : base(() => 
            new Fixture()
            .Customize(new AutoMoqCustomization())
            .Customize(new CommonCustomization())
            .Customize(new FixtureCustomizations())
            .UseMapster())       
        {

        }
    }
}
