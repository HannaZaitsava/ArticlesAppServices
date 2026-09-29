using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCommands.CreateArticle
{   
    public sealed record CreateArticleCommand: IRequest<Result<ArticleResponseDTO>>
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;

        public IReadOnlyCollection<Guid>? Categories { get; set; }
        public IReadOnlyCollection<Guid>? Tags { get; set; }
    }
}
