using System.Linq.Expressions;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.Application.RequestFeatures.CursorPagination;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Application.RequestFeatures.Sorting;
using ArticlesService.Domain.Entities;

namespace ArticlesService.Application.Abstractions.DataAccess
{
    public interface IArticleRepository: IBaseRepository<Article>
    {        
        Task<Article?> GetArticleWithFullInfoAsync(
            Guid id, 
            bool trackChanges = true, 
            CancellationToken ct = default);

        Task<Article?> GetArticleInfoWihoutCommentsAsync(
            Guid id,
            bool trackChanges = true,
            CancellationToken ct = default);

        Task<OffsetPagedResult<TDestinationDTO>> GetArticleCommentsOffsetPaginatedProjectedAsync<TDestinationDTO>(
            Guid articleId, 
            OffsetPaginationParameters paginationParameters, 
            Expression<Func<Comment, bool>>? filterPredicate = null, 
            CancellationToken ct = default);

        Task<CursorPagedResult<CommentResponseDTO>> GetArticleCommentsCursorPaginatedProjectedAsync(
             Guid articleId,
             CursorPaginationParameters paginationParameters,
             Expression<Func<Comment, bool>>? predicate = null,
             CancellationToken ct = default);

        Task<OffsetPagedResult<ArticleShortInfoResponseDTO>> GetArticlesOffsetPagedListProjectedAsync<ArticleShortInfoResponseDTO>(
            OffsetPaginationParameters paginationParameters, 
            ArticleSortItem? sort = null, 
            Expression<Func<Article, bool>>? filterPredicate = null, 
            CancellationToken ct = default);
    }
}
