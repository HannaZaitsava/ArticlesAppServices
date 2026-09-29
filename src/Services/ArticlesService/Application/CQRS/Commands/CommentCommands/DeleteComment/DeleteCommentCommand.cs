using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.CommentCommands.DeleteComment
{    
    public sealed record DeleteCommentCommand(Guid Id) : IRequest<Result<bool>>;
}
