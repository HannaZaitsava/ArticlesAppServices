using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleQueries.GetArticleQuery
{
    public sealed record GetArticleQuery(Guid Id) : 
        IRequest<Result<ArticleResponseDTO>>,
        ICachableRequest
    {
        public string GetCacheKeyMetadata() => CacheKeys.Article(Id);

        public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.Article(Id), Common.Caching.CacheTags.Articles];        
    }
}
