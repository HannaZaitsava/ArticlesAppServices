using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCommands.DeleteArticle
{
    public sealed record DeleteArticleCommand(Guid Id) : IRequest<Result<bool>>;
}
