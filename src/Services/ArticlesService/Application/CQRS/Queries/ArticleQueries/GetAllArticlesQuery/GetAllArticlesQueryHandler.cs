using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleQueries.GetAllArticlesQuery
{
    internal class GetAllArticlesQueryHandler(IArticleRepository articleRepository) : 
        IRequestHandler<GetAllArticlesQuery, 
            Result<OffsetPagedResult<ArticleShortInfoResponseDTO>>>
    {        
        public async Task<Result<OffsetPagedResult<ArticleShortInfoResponseDTO>>> Handle(GetAllArticlesQuery request, CancellationToken cancellationToken)
        {    
            var articles = await articleRepository.GetArticlesOffsetPagedListProjectedAsync<ArticleShortInfoResponseDTO>(
                sort: request.Sorts,
                paginationParameters: request.PaginationParameters,
                ct: cancellationToken);
            
            return Result<OffsetPagedResult<ArticleShortInfoResponseDTO>>.Success(articles);
        }
    }
}
