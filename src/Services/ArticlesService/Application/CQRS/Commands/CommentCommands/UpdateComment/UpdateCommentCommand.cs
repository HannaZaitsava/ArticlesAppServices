using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.CommentCommands.UpdateComment
{
    public sealed record UpdateCommentCommand(Guid Id, string Text) : IRequest<Result<bool>>;
}
