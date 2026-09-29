using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.TagCommands.DeleteTag
{
    internal class DeleteTagCommandHandler(
        ITagRepository repository,
        ICacheInvalidationContext cacheContext) 
        : IRequestHandler<DeleteTagCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
        {
            var tagId = request.Id;

            var tagEntity = await repository.GetTagWithFullInfoAsync(tagId, true, cancellationToken);

            if (tagEntity is null)
            {
                return Result<bool>.Failure([TagErrors.TagNotFound(tagId)]);
            }

            // Cache tags to invalidate           
            cacheContext.AddTags([
                CacheTags.Tags,
                CacheTags.Tag(request.Id)
                ]);

            if (tagEntity.Articles is not null)
            {
                foreach (var article in tagEntity.Articles)
                    cacheContext.AddTag(CacheTags.Article(article.Id));            
            }

            repository.Remove(tagEntity);
            await repository.SaveChangesAsync(cancellationToken); 
            
            return Result<bool>.Success(true);
        }
    }
}
