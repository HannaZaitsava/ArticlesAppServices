using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCommands.PublishArticleCommand
{
    public sealed record PublishArticleCommand(Guid Id) : IRequest<Result<bool>>;
}
