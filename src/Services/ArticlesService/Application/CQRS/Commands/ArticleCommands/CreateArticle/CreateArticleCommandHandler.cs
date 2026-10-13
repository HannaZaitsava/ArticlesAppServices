using ArticlesService.Application.Abstractions;
using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Helpers;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Domain.Constants.EntityConstraints;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MapsterMapper;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCommands.CreateArticle
{  
    internal class CreateArticleCommandHandler(
        IBaseRepository<Article> articleRepository,
        IBaseRepository<ArticleCategory> articleCategoryRepository,
        IBaseRepository<Tag> tagRepository,
        IUserContext userContext,
        ICacheInvalidationContext cacheContext,
        IMapper mapper)
        : IRequestHandler<CreateArticleCommand, Result<ArticleResponseDTO>>
    {
        public async Task<Result<ArticleResponseDTO>> Handle(CreateArticleCommand request, CancellationToken cancellationToken)
        {            
            var exists = await articleRepository.IsExistingAsync(a => a.Title == request.Title, cancellationToken);
            
            if (exists)
            {
                return Result<ArticleResponseDTO>.Failure([ArticleErrors.ArticleAlreadyExists(request.Title)]);
            }

            var article = mapper.Map<Article>(request);

            // TODO: пеервести анемичную доменную модель на богатую. Перенести урезание контента в метод SetPreviewText самой сущности
            if (!string.IsNullOrWhiteSpace(article.Content))
            {
                // Очищаем контент статьи от разметки и небезопасных инъекций
                string plainText = ArticleContextSanitizer.ToPlainText(article.Content);

                // Обрезаем до лимита из ваших констант ArticleConstraints
                article.PreviewText = plainText.Length > ArticleConstraints.PreviewTextMaxLength
                    ? plainText.Substring(0, ArticleConstraints.PreviewTextMaxLength) + "..."
                    : plainText;
            }
            else
            {
                article.PreviewText = null;
            }

            if (request.Tags is { Count: > 0 })
            {
                var foundTags = await tagRepository.GetAllAsync(t => request.Tags.Contains(t.Id), true, cancellationToken);

                var invalidIds = request.Tags.Except(foundTags.Select(t => t.Id));

                if (invalidIds.Any())
                { 
                    return Result<ArticleResponseDTO>.Failure([TagErrors.TagsNotFound(invalidIds)]);
                }
                              
                article.Tags = (ICollection<Tag>)foundTags;
            }

            if (request.Categories is { Count: > 0 })
            {                
                var foundCategories = await articleCategoryRepository.GetAllAsync(t => request.Categories.Contains(t.Id), true, cancellationToken);

                var invalidIds = request.Categories.Except(foundCategories.Select(t => t.Id));

                if (invalidIds.Any())
                {
                    return Result<ArticleResponseDTO>.Failure([ArticleCategoryErrors.ArticleCategoriesNotFound(invalidIds)]);
                }

                article.Categories = (ICollection<ArticleCategory>)foundCategories;
            }
         
            await articleRepository.AddAsync(article, cancellationToken);
            await articleRepository.SaveChangesAsync(cancellationToken);

            // Cache tags to invalidate
            cacheContext.AddTag(CacheTags.Articles);            

            if (request.Categories is not null)
            {
                foreach (var сategory in request.Categories)
                    cacheContext.AddTag(CacheTags.ArticleCategory(сategory));
            }

            if (request.Tags is not null)
            {
                foreach (var tag in request.Tags)
                    cacheContext.AddTag(CacheTags.Tag(tag));
            }
            
            var articleResponseDTO = mapper.Map<ArticleResponseDTO>((Article: article, CreatorName: userContext.UserName));

            return Result<ArticleResponseDTO>.Success(articleResponseDTO);
        }
    }
}
