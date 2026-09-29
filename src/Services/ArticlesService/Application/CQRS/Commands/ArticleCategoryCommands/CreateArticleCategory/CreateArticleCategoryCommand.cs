using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.CreateArticleCategory
{
    public sealed record CreateArticleCategoryCommand(string Name) : IRequest<Result<ArticleCategoryResponseDTO>>;
}
