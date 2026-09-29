using ArchUnitNET.xUnit;
using MediatR;
using static ArchUnitNET.Fluent.ArchRuleDefinition;


namespace ArticlesService.Tests.ArchitectureTests
{
    public class VisibilityTests : BaseTest
    {
        [Fact]
        public void RequestHandlers_ShouldBeInternal()
        {
            Classes().That()               
                .ImplementInterface(typeof(IRequestHandler<,>))
                // Меняет текст ошибки. Без .As(): "Classes that implement IRequestHandler`2 should be internal."
                .As("MediatR Handlers")  
                .Should().BeInternal()
                .Check(Architecture);
        }
    }
}
