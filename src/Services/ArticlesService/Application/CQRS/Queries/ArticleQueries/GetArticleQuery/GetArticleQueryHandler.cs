using ArticlesService.Application.Abstractions;
using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Enums;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleQueries.GetArticleQuery
{
    internal class GetArticleQueryHandler(
        IBaseRepository<Article> repository,
        IUserContext userContext) 
        : IRequestHandler<GetArticleQuery, Result<ArticleResponseDTO>>
    {
        public async Task<Result<ArticleResponseDTO>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
        {
            Guid articleId = request.Id;
            
            var articleResponseDTO = await repository.GetByIdProjectedAsync<ArticleResponseDTO>(articleId, cancellationToken);

            if (articleResponseDTO is null)
            {
                return Result<ArticleResponseDTO>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            }

            // Если статья опубликована, её могут читать все (даже неавторизованные)
            if (articleResponseDTO.Status == ArticleStatus.Published)
            {
                return Result<ArticleResponseDTO>.Success(articleResponseDTO);
            }

            // Если статья НЕ опубликована, проверяем, авторизован ли пользователь и является ли он автором
            if (!userContext.IsAuthenticated || articleResponseDTO.CreatedBy != userContext.UserId)
            {
                // Можно вернуть 403, но возвращаем NotFound для безопасности, чтобы скрыть сам факт существования черновика.
                // Чтобы злоумышленник не мог перебором GUID узнать, существует ли статья в системе
                return Result<ArticleResponseDTO>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            }            

            return Result<ArticleResponseDTO>.Success(articleResponseDTO);
        }
    }
}
