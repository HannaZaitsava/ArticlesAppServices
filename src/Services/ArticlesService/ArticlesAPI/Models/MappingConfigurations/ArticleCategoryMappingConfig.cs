using ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.UpdateArticleCategory;
using ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetAllArticleCategories;
using ArticlesService.ArticlesAPI.Models.Requests;
using Mapster;

namespace ArticlesService.ArticlesAPI.Models.MappingConfigurations
{
    public class ArticleCategoryMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {           
            config.NewConfig<(Guid id, ArticleCategoryApiRequest requestBody), UpdateArticleCategoryCommand>()
               .Map(dest => dest.Id, src => src.id) 
               .Map(dest => dest, src => src.requestBody);

            config.NewConfig<GetAllCategoriesPaginatedApiRequest, GetAllArticleCategoriesQuery>()
             .Map(dest => dest.PaginationParameters.PageIndex, src => src.PageIndex)
             .Map(dest => dest.PaginationParameters.PageSize, src => src.PageSize);
        }
    }
}
