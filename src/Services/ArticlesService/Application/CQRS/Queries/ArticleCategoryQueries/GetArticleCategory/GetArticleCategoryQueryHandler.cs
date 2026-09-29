using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetArticleCategory
{
    internal class GetArticleCategoryQueryHandler(IBaseRepository<ArticleCategory> repository) : IRequestHandler<GetArticleCategoryQuery, Result<ArticleCategoryResponseDTO>>
    {
        public async Task<Result<ArticleCategoryResponseDTO>> Handle(GetArticleCategoryQuery request, CancellationToken cancellationToken)
        {
            var articleCategoryId = request.Id;
           
            var articleCategory = await repository.GetByIdProjectedAsync<ArticleCategoryResponseDTO>(articleCategoryId, cancellationToken);

            if (articleCategory is null)
            {
                return Result<ArticleCategoryResponseDTO>.Failure([ArticleCategoryErrors.ArticleCategoryNotFound(articleCategoryId)]);
            }

            return Result<ArticleCategoryResponseDTO>.Success(articleCategory);
        }
    }
}
