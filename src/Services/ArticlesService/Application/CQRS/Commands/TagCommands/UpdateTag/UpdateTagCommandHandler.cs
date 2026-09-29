using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MapsterMapper;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.TagCommands.UpdateTag
{
    internal class UpdateTagCommandHandler(
        ITagRepository repository,
        ICacheInvalidationContext cacheContext,
        IMapper mapper) 
        : IRequestHandler<UpdateTagCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
        {
            var tagId = request.Id;

            var tagEntity = await repository.GetTagWithFullInfoAsync(tagId, true, cancellationToken);

            if (tagEntity is null)
            {
                return Result<bool>.Failure([TagErrors.TagNotFound(tagId)]);
            }

            mapper.Map(request, tagEntity);

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
            
            await repository.SaveChangesAsync(cancellationToken);            
                        
            return Result<bool>.Success(true);
        }
    }
}
