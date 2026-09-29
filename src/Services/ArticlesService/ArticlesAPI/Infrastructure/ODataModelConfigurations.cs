using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace ArticlesService.ArticlesAPI.Infrastructure
{
    public static class ODataModelConfiguration
    {
        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();

            // Настраиваем EntitySets для DTO
            //builder.EntitySet<ArticleRequest>("Articles");
            //builder.EntitySet<ArticleCategoryDto>("ArticleCategories");

            // Если нужно настроить ключи или игнорировать поля:
            //var article = builder.EntityType<ArticleRequest>();
            //article.HasKey(p => p.Id);
            // article.Ignore(p => p.InternalCode);

            return builder.GetEdmModel();
        }
    }
}
