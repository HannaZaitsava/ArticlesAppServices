using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MapsterMapper;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.CreateArticleCategory
{
    internal class CreateArticleCategoryCommandHandler(
        IBaseRepository<ArticleCategory> repository,
        ICacheInvalidationContext cacheContext,
        IMapper mapper) 
        : IRequestHandler<CreateArticleCategoryCommand, Result<ArticleCategoryResponseDTO>>
    {
        public async Task<Result<ArticleCategoryResponseDTO>> Handle(CreateArticleCategoryCommand request, CancellationToken cancellationToken)
        {
            var exists = await repository.IsExistingAsync(t => t.Name == request.Name, cancellationToken);

            if (exists)
            {
                return Result<ArticleCategoryResponseDTO>.Failure([ArticleCategoryErrors.ArticleCategoryAlreadyExists(request.Name)]);
            }

            var articleCategory = mapper.Map<ArticleCategory>(request);

            await repository.AddAsync(articleCategory, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            cacheContext.AddTag(CacheTags.ArticleCategories);

            var responseDto = mapper.Map<ArticleCategoryResponseDTO>(articleCategory);

            return Result<ArticleCategoryResponseDTO>.Success(responseDto);
        }
    }
}
