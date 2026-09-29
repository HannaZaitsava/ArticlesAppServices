using ArticlesService.Application.CQRS.Commands.ArticleCommands.UpdateArticle;
using ArticlesService.Application.CQRS.Queries.ArticleQueries.GetAllArticlesQuery;
using ArticlesService.ArticlesAPI.Models.Requests;
using Mapster;

namespace ArticlesService.ArticlesAPI.Models.MappingConfigurations
{
    public class ArticleMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<(Guid id, ArticleApiRequest requestBody), UpdateArticleCommand>()
               .Map(dest => dest.Id, src => src.id) 
               .Map(dest => dest, src => src.requestBody);

            config.NewConfig<GetArticlesPaginatedApiRequest, GetAllArticlesQuery>()
            .Map(dest => dest.PaginationParameters.PageIndex, src => src.PageIndex)
            .Map(dest => dest.PaginationParameters.PageSize, src => src.PageSize);
        }
    }
}
