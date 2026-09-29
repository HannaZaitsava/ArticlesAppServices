using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using AutoFixture;

namespace ArticlesService.Tests.IntegrationTests.FixtureCustomizations
{    
    public class TagsCustomization : ICustomization
    {
        /*
         Настроийка генерации валидных данных по умолчанию, а для негативных тестов (где нужны ошибки валидации) — переопределять значения вручную.
         */
        public void Customize(IFixture fixture)
        {
            fixture.Customize<Tag>(с => с
            .Without(t => t.Articles)

            .With(t => t.Label, () => fixture.Create<string>()[..Math.Min(TagConstraints.MaxLabelLength, fixture.Create<string>().Length)])

            .With(t => t.Color, () => $"#{fixture.Create<int>():X6}"[..TagConstraints.MaxColorLength])
        );

            //fixture.Customize<Tag>(c => c
            //    .Do(e => e.Articles = new List<Article>())); 
        }
    }
}
