using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetArticleCategory
{
    public sealed record GetArticleCategoryQuery(Guid Id) : 
        IRequest<Result<ArticleCategoryResponseDTO>>,
        ICachableRequest
    {
        public string GetCacheKeyMetadata() => CacheKeys.ArticleCategory(Id);

        public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.ArticleCategory(Id), Common.Caching.CacheTags.ArticleCategories];
    }
}
