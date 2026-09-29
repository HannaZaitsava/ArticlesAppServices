using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.CommentQueries.GetComment
{
    public sealed record GetCommentQuery(Guid Id) : 
        IRequest<Result<CommentResponseDTO>>,
        ICachableRequest
    {
        public string GetCacheKeyMetadata() => CacheKeys.Comment(Id);

        public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.Comment(Id), Common.Caching.CacheTags.Comments];
    }
}
