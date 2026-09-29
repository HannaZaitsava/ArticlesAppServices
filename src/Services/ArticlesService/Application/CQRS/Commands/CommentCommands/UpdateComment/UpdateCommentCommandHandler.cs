using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MapsterMapper;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.CommentCommands.UpdateComment
{
    internal class UpdateCommentCommandHandler(
        IBaseRepository<Comment> repository,
        ICacheInvalidationContext cacheContext,
        IMapper mapper)
        : IRequestHandler<UpdateCommentCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var commentId = request.Id;

            var commentEntity = await repository.GetByIdAsync(commentId, true, cancellationToken);

            if (commentEntity is null)
            {
                return Result<bool>.Failure([CommentErrors.CommentNotFound(commentId)]);
            }

            mapper.Map(request, commentEntity);

            await repository.SaveChangesAsync(cancellationToken);

            // Cache Comments to invalidate
            cacheContext.AddTag(CacheTags.Comment(commentId));
           
            if (commentEntity.ParentId is not null)
                cacheContext.AddTag(CacheTags.Comment((Guid)commentEntity.ParentId));

            // TODO: возможно, заменить инвалидацию кэша на паттерн Cache Update (Push в кэш): не инвалидировать тег, а асинхронно дописать (делает push) 
            // комментарий прямо в существующий закэшированный список комментариев в Redis (например, если кэш хранится в виде JSON-массива или структуры данных Redis List).
            
            return Result<bool>.Success(true);
        }
    }
}
