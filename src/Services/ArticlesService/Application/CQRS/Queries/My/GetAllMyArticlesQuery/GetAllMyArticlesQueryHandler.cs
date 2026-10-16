using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.My.GetAllMyArticlesQuery
{  
    internal class GetAllMyArticlesQueryHandler(
        IArticleRepository articleRepository) :
        IRequestHandler<GetAllMyArticlesQuery,
        Result<OffsetPagedResult<ArticleShortInfoResponseDTO>>>
    {
        public async Task<Result<OffsetPagedResult<ArticleShortInfoResponseDTO>>> Handle(GetAllMyArticlesQuery request, CancellationToken cancellationToken)
        {
            var articles = await articleRepository.GetArticlesOffsetPagedListProjectedAsync<ArticleShortInfoResponseDTO>(
                sort: request.Sorts,
                paginationParameters: request.PaginationParameters,
                filterPredicate: article => article.Status == request.Status && article.AuthorId == request.UserContext.UserId,
                ct: cancellationToken);

            return Result<OffsetPagedResult<ArticleShortInfoResponseDTO>>.Success(articles);
        }
    }
}
