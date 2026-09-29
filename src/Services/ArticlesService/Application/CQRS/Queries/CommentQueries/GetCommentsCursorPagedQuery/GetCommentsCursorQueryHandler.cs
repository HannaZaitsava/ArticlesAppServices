using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.Application.Extensions;
using ArticlesService.Application.RequestFeatures.CursorPagination;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsCursorPagedQuery
{
    internal class GetCommentsCursorQueryHandler(
        IArticleRepository articlesRepository,
        ICommentRepository commentRepository)
        : IRequestHandler<GetCommentsCursorQuery, 
          Result<CursorPagedResult<CommentResponseDTO>>>
    {
        public async Task<Result<CursorPagedResult<CommentResponseDTO>>> Handle(GetCommentsCursorQuery request, CancellationToken cancellationToken)
        {
            var articleId = request.ArticleId;

            var isArticleExists = await articlesRepository.IsExistingAsync(t => t.Id == articleId, cancellationToken);

            if (isArticleExists == false)
            {
                return Result<CursorPagedResult<CommentResponseDTO>>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            }

            // Получаем только ID корневых комментариев для текущей страницы            
            var rootCommentIdsPagedResult = await articlesRepository.GetArticleCommentsCursorPaginatedProjectedAsync(
                articleId, 
                request.PaginationParameters, 
                c => c.RootCommentId == null,
                cancellationToken);

            var rootIds = rootCommentIdsPagedResult.Items.Select(c => c.Id);

            // Выгружаем вложенные элементы по RootId
            var nestedCommentsPagesResult = await commentRepository.GetNestedCommentsProjectedAsync<CommentResponseDTO>(rootIds, cancellationToken);
            //var nestedCommentsPagesResult = await articlesRepository.GetArticleCommentsProjectedAsync(
            //    articleId, 
            //    c => c.RootCommentId != null && rootIds.Contains(c.RootCommentId.Value), 
            //    cancellationToken);

            var allComments = rootCommentIdsPagedResult.Items;
                //rootCommentIdsPagedResult.Items.Union(nestedCommentsPagesResult);

            /*  TODO:
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

            return Result<CursorPagedResult<CommentResponseDTO>>.Success(finalTreeResult);
        }
    }
}
