using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MapsterMapper;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.UpdateArticleCategory
{   
     internal class UpdateArticleCategoryCommandHandler(
        IArticleCategoryRepository repository,
        ICacheInvalidationContext cacheContext,
        IMapper mapper) 
        : IRequestHandler<UpdateArticleCategoryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateArticleCategoryCommand request, CancellationToken cancellationToken)
        {
            var articleCategoryId = request.Id;

            var articleCategoryEntity = await repository.GetArticleCategoryWithFullInfoAsync(articleCategoryId, ct: cancellationToken);

            if (articleCategoryEntity is null)
            {
                return Result<bool>.Failure([ArticleCategoryErrors.ArticleCategoryNotFound(articleCategoryId)]);
            }

            mapper.Map(request, articleCategoryEntity);            

            cacheContext.AddTags([
                CacheTags.ArticleCategories, 
                CacheTags.ArticleCategory(request.Id)
                ]);
            
            if (articleCategoryEntity.Articles is not null)
            {
                foreach (var article in articleCategoryEntity.Articles)
                    cacheContext.AddTag(CacheTags.Article(article.Id));
            }

            await repository.SaveChangesAsync(cancellationToken);            

            return Result<bool>.Success(true);
        }
    }
}
