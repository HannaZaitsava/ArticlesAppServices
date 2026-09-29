using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleQueries.GetArticleQuery
{
    internal class GetArticleQueryHandler(IBaseRepository<Article> repository) : IRequestHandler<GetArticleQuery, Result<ArticleResponseDTO>>
    {
        public async Task<Result<ArticleResponseDTO>> Handle(GetArticleQuery request, CancellationToken cancellationToken)
        {
            Guid articleId = request.Id;
            
            var articleResponseDTO = await repository.GetByIdProjectedAsync<ArticleResponseDTO>(articleId, cancellationToken);
                       
            if (articleResponseDTO is null)
            {
                return Result<ArticleResponseDTO>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            }

            return Result<ArticleResponseDTO>.Success(articleResponseDTO);
        }
    }
}
