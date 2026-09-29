using AutoFixture.Xunit2;

namespace ArticlesService.Tests.UnitTests.Attributes
{
    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {        
        public InlineAutoMoqDataAttribute(params object[] values)
            : base(new AutoMoqDataAttribute(), values)
        {
        }
    }
}
