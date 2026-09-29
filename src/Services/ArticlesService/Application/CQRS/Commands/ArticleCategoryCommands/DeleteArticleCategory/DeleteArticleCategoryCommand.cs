using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.DeleteArticleCategory
{   
    public sealed record DeleteArticleCategoryCommand(Guid Id) : IRequest<Result<bool>>;
}
