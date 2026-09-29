using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.TagQueries.GetTag
{   
    public sealed record GetTagQuery(Guid Id) : 
        IRequest<Result<TagResponseDTO>>,
        ICachableRequest
    {
        public string GetCacheKeyMetadata() => CacheKeys.Tag(Id);

        public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.Tag(Id), Common.Caching.CacheTags.Tags];
    }
}
