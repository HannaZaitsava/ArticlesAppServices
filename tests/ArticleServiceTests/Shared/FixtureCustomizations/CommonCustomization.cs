using AutoFixture;

namespace ArticlesService.Tests.Shared.FixtureCustomizations
{
    public class CommonCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            // Обработка циклических ссылок (рекурсии)
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            // Поддержка перечислений (Enum) и других системных типов, 
            // если стандартный генератор на них спотыкается
            // ...
        }
    }
}
