using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.Common.Caching;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.CommentCommands.DeleteComment
{
    internal class DeleteCommentCommandHandler(
        IBaseRepository<Comment> repository,
        ICacheInvalidationContext cacheContext)
        : IRequestHandler<DeleteCommentCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var commentId = request.Id;

            var commentEntity = await repository.GetByIdAsync(commentId, trackChanges: true, cancellationToken);

            if (commentEntity is null)
            {
                return Result<bool>.Failure([CommentErrors.CommentNotFound(commentId)]);
            }

            repository.Remove(commentEntity);
            await repository.SaveChangesAsync(cancellationToken);

            // Cache tags to invalidate
            cacheContext.AddTag(CacheTags.Comment(commentId));
           
            if (commentEntity.ParentId is not null)
                cacheContext.AddTag(CacheTags.Comment((Guid)commentEntity.ParentId));

            /// TODO: возможно, заменить инвалидацию кэша на паттерн Cache Update (Push в кэш): не инвалидировать тег, а асинхронно дописать (делает push) 
            /// комментарий прямо в существующий закэшированный список комментариев в Redis (например, если кэш хранится в виде JSON-массива или структуры данных Redis List).
            
            return Result<bool>.Success(true);
        }
    }
}
