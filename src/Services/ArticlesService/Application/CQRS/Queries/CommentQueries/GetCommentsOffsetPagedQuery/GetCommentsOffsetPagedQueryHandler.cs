using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.Application.Extensions;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsOffsetPagedQuery
{
    internal class GetCommentsOffsetPagedQueryHandler(
        IArticleRepository articleRepository,
        ICommentRepository commentRepository) 
        : IRequestHandler<GetCommentsOffsetPagedQuery, Result<OffsetPagedResult<CommentResponseDTO>>>
    {
        public async Task<Result<OffsetPagedResult<CommentResponseDTO>>> Handle(GetCommentsOffsetPagedQuery request, CancellationToken cancellationToken)
        {           
            var articleId = request.ArticleId;

            var isArticleExists = await articleRepository.IsExistingAsync(t => t.Id == articleId, cancellationToken);

            if (isArticleExists == false)
            {
                return Result<OffsetPagedResult<CommentResponseDTO>>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            }            

            var rootCommentIdsPagedResult = await articleRepository.GetArticleCommentsOffsetPaginatedProjectedAsync<CommentResponseDTO>(
                //new List<(System.Linq.Expressions.Expression<Func<ArticleCategory, object>> KeySelector, bool IsDescending)>()
                //sortOrder: (comment => comment.CreatedOn, false),
                articleId,                
                request.PaginationParameters,
                filterPredicate : c => c.RootCommentId == null,
                cancellationToken);

            var rootIds = rootCommentIdsPagedResult.Items.Select(c => c.Id);

            // Выгружаем сами корни и все их дочерние элементы по RootId
            //var commentsByRootIdsSpec = new CommentsByRootIdsSpec(rootIds);
            //var allCommentsPagesResult = await baseRepository.GetPagedListBySpecAsync<CommentResponseDTO>(commentsByRootIdsSpec, cancellationToken);
            var nestedCommentsPagesResult = await commentRepository.GetNestedCommentsProjectedAsync<CommentResponseDTO>(rootIds, cancellationToken);

            var allComments = rootCommentIdsPagedResult.Items.Union(nestedCommentsPagesResult);

            /*  TODO
                Вместо того чтобы строить огромные деревья в GET, часто используют плоский список с метаданными:
                 - Фронтенд получает список комментариев, где у каждого есть ParentId.
                 - Фронтенд сам собирает дерево (на клиенте это дешевле, чем на сервере).
                 - Для «бесконечных» тредов используется Lazy Loading: подгружаем N ответов, а дальше кнопка «Показать еще М ответов», которая вызывает GET /api/comments/{id}/replies?page=2.
            */
            // собираем в дерево в памяти
            //var commentsTree = allCommentsPagesResult.Items.ConvertToTree();
            var commentsTree = allComments.ConvertToTree();

            // Сохраняем иммутабельность
            var finalTreeResult = rootCommentIdsPagedResult.Map(commentsTree);

            return Result<OffsetPagedResult<CommentResponseDTO>>.Success(finalTreeResult);
        }
    }
}