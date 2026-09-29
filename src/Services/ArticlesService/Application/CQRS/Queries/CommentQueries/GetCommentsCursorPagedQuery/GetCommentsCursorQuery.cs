using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.Application.Enums;
using ArticlesService.Application.RequestFeatures.CursorPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsCursorPagedQuery
{    
    public sealed record GetCommentsCursorQuery
        : IRequest<Result<CursorPagedResult<CommentResponseDTO>>>,
          ICachableRequest
    {
        public Guid ArticleId { get; init; }

        public CursorPaginationParameters PaginationParameters { get; init; } = new()
        {
            Cursor = null,
            PageSize = PaginationConstants.CommentsDefaultPageSize,
            Direction = PaginationDirection.Forward
        };                
                
        public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.ArticleComments(ArticleId), Common.Caching.CacheTags.Comments];

        public string GetCacheKeyMetadata() =>
            $"article:{ArticleId}:cursor:{PaginationParameters.Cursor ?? "first"}:size:{PaginationParameters.PageSize}:dir:{PaginationParameters.Direction}";               
    }
}
