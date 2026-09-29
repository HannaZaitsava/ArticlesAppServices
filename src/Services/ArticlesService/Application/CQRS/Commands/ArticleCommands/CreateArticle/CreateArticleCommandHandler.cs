using ArticlesService.Application.Common.Events;
using MapsterMapper;
using MediatR;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using ArticlesService.Application.Abstractions;
using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Application.Common.Caching;

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

            // Маппим основные поля (Title, Content)
            var article = mapper.Map<Article>(request);
            
            // Загружаем связанные сущности из БД по списку ID
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
