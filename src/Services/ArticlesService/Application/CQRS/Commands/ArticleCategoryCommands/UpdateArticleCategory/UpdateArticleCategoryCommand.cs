using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.UpdateArticleCategory
{   
    public sealed record UpdateArticleCategoryCommand(Guid Id, string Name) : IRequest<Result<bool>>;
}
