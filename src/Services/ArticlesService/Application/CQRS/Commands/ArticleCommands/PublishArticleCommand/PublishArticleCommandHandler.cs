using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCommands.PublishArticleCommand
{
    internal class PublishArticleHandler(
        IArticleRepository articleRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ICacheInvalidationContext cacheContext) 
        : IRequestHandler<PublishArticleCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(PublishArticleCommand request, CancellationToken cancellationToken)
        {
            var articleId = request.Id;

            // комментарии подгружать не нужно (их много => убьет производительность)
            var articleEntity = await articleRepository.GetArticleInfoWihoutCommentsAsync(articleId, true, cancellationToken);

            if (articleEntity is null)
                return Result<bool>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            
            var publishingError = articleEntity.Publish(timeProvider.GetUtcNow());

            if(publishingError is not null)
                return Result<bool>.Failure([publishingError]);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Cache tags to invalidate            
            cacheContext.AddTags([
                 CacheTags.Articles,
                 CacheTags.Article(articleId),
                 CacheTags.ArticleComments(articleId)
                ]);

            if (articleEntity.Categories is not null)
            {
                foreach (var сategory in articleEntity.Categories)
                    cacheContext.AddTag(CacheTags.ArticleCategory(сategory.Id));
            }

            if (articleEntity.Tags is not null)
            {
                foreach (var tag in articleEntity.Tags)
                    cacheContext.AddTag(CacheTags.Tag(tag.Id));
            }

            return Result<bool>.Success(true);           
        }
    }
}
