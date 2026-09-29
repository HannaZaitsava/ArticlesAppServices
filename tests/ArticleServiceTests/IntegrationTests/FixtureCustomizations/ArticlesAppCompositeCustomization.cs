using ArticlesService.Tests.IntegrationTests.FixtureCustomizations;
using ArticlesService.Tests.Shared.FixtureCustomizations;
using AutoFixture;

namespace ArticlesService.Tests.IntegrationTests.FixtureCustomizations
{
    public class ArticlesAppCompositeCustomization : CompositeCustomization
    {
        public ArticlesAppCompositeCustomization()
            : base(
                new CommonCustomization(),
                new TagsCustomization())
        {
        }
    }   
}
