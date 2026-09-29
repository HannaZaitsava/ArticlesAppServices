using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetAllArticleCategories
{
    internal class GetAllArticleCategoriesQueryHandler(
        IArticleCategoryRepository categoryRepository)
        : IRequestHandler<GetAllArticleCategoriesQuery, Result<OffsetPagedResult<ArticleCategoryShotrInfoResponseDTO>>>
    {
        public async Task<Result<OffsetPagedResult<ArticleCategoryShotrInfoResponseDTO>>> Handle(GetAllArticleCategoriesQuery request, CancellationToken cancellationToken)
        {            
            var articleCategories = await categoryRepository.GetOffsetPagedListProjectedAsync<ArticleCategoryShotrInfoResponseDTO>(
                paginationParameters: request.PaginationParameters,
                cancellationToken);

            return Result<OffsetPagedResult<ArticleCategoryShotrInfoResponseDTO>>.Success(articleCategories); 
        }
    }
}
