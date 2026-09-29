using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Errors;
using ArticlesService.Domain.Result;
using MapsterMapper;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.CommentCommands.CreateComment
{
   internal class CreateCommentCommandHandler(
       IBaseRepository<Comment> commentRepository,
       IBaseRepository<Article> articleRepository,
       IMapper mapper) 
        : IRequestHandler<CreateCommentCommand, Result<Guid>>
    {       
        public async Task<Result<Guid>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
        {
            var articleId = request.ArticleId;

            var isArticleExists = await articleRepository.IsExistingAsync(a => a.Id == articleId, cancellationToken);

            if (isArticleExists == false)
            {
                return Result<Guid>.Failure([ArticleErrors.ArticleNotFound(articleId)]);
            }

            var comment = mapper.Map<Comment>(request);

            if (request.ParentId is Guid parentId)
            {
                // Ищем родителя, чтобы узнать его RootCommentId
                var parent = await commentRepository.GetByIdAsync(parentId, false, cancellationToken);
                                             
                if(parent is null)
                    return Result<Guid>.Failure([CommentErrors.ParentCommentNotFound(parentId)]);

                // Валидация: нельзя ответить на комментарий из другой статьи
                if (parent.ArticleId != request.ArticleId)
                    return Result<Guid>.Failure([CommentErrors.CommentDoesNotBelongToTheSpecifiedArticle(request.ArticleId)]);

                // Если у родителя RootCommentId пустой, значит ЭТОТ РОДИТЕЛЬ САМ является корнем.
                // Если не пустой — берем его значение.
                comment.RootCommentId = parent.RootCommentId ?? parentId;
            }
            //else
            //{
            //    // Это корневой комментарий, RootCommentId остается null (или равен самому себе)
            //    comment.RootCommentId = null;
            //}

            await commentRepository.AddAsync(comment, cancellationToken);
            await commentRepository.SaveChangesAsync(cancellationToken);

            // TODO: возможно, заменить инвалидацию кэша на паттерн Cache Update (Push в кэш): не инвалидировать тег, а асинхронно дописать (делает push) 
            // комментарий прямо в существующий закэшированный список комментариев в Redis (например, если кэш хранится в виде JSON-массива или структуры данных Redis List).

            return Result<Guid>.Success(comment.Id);
        }
    }
}
