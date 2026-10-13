using ArticlesService.Application.Abstractions;
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
        IUserContext userContext,
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

            if (!userContext.IsAuthenticated || articleEntity.CreatedBy != userContext.UserId)
            {
                // Можно вернуть 403, но возвращаем NotFound для безопасности, чтобы скрыть сам факт существования статьи.
                // Чтобы злоумышленник не мог перебором GUID узнать, существует ли статья в системе
                return Result<bool>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            }

            var publishResult = articleEntity.Publish(timeProvider.GetUtcNow());

            if (publishResult.IsFailure)
                return publishResult;

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

            return publishResult;           
        }
    }
}
