using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.DeleteArticleCategory
{
    internal class DeleteArticleCategoryCommandHandler(
        IArticleCategoryRepository repository,
        ICacheInvalidationContext cacheContext)
        : IRequestHandler<DeleteArticleCategoryCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteArticleCategoryCommand request, CancellationToken cancellationToken)
        {
            var articleCategoryId = request.Id;
           
            var articleCategoryEntity = await repository.GetArticleCategoryWithFullInfoAsync(articleCategoryId, true, cancellationToken);

            if (articleCategoryEntity is null)
            {
                return Result<bool>.Failure([ArticleCategoryErrors.ArticleCategoryNotFound(articleCategoryId)]);
            }           

            cacheContext.AddTags([
                CacheTags.ArticleCategories, 
                CacheTags.ArticleCategory(request.Id)
                ]);

            if (articleCategoryEntity.Articles is not null)
            {
                foreach (var article in articleCategoryEntity.Articles)
                    cacheContext.AddTag(CacheTags.Article(article.Id));
            }

            repository.Remove(articleCategoryEntity);
            await repository.SaveChangesAsync(cancellationToken); 

            return Result<bool>.Success(true);
        }
    }
}
