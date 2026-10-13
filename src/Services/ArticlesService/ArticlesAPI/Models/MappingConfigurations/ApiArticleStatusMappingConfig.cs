using ArticlesService.ArticlesAPI.Models.Enums;
using ArticlesService.Domain.Enums;
using Mapster;

namespace ArticlesService.ArticlesAPI.Models.MappingConfigurations
{
    public class ArticleStatusMappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ApiArticleStatus, ArticleStatus>()
                .MapWith(src => ToDomain(src));

            config.NewConfig<ArticleStatus, ApiArticleStatus>()
                .MapWith(src => ToApi(src));
        }

        private static ArticleStatus ToDomain(ApiArticleStatus src) => src switch
        {
            ApiArticleStatus.Draft => ArticleStatus.Draft,
            ApiArticleStatus.PendingReview => ArticleStatus.PendingReview,
            ApiArticleStatus.Published => ArticleStatus.Published,
            ApiArticleStatus.Archived => ArticleStatus.Archived,
            ApiArticleStatus.Banned => ArticleStatus.Banned,
            ApiArticleStatus.Deleted => ArticleStatus.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(src), $"Unsupported Article status: {src}")
        };

        private static ApiArticleStatus ToApi(ArticleStatus src) => src switch
        {
            ArticleStatus.Draft => ApiArticleStatus.Draft,
            ArticleStatus.PendingReview => ApiArticleStatus.PendingReview,
            ArticleStatus.Published => ApiArticleStatus.Published,
            ArticleStatus.Archived => ApiArticleStatus.Archived,
            ArticleStatus.Banned => ApiArticleStatus.Banned,
            ArticleStatus.Deleted => ApiArticleStatus.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(src), $"Unknown domain Article status: {src}")
        };
    }
}
