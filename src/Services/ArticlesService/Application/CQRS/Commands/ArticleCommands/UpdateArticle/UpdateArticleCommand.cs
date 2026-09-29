using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.ArticleCommands.UpdateArticle
{
    public sealed record UpdateArticleCommand() : IRequest<Result<bool>>
    {
        public Guid Id { get; set; }
        public string? Title { get; set; } 
        public string? Content { get; set; } 

        public IReadOnlyCollection<Guid>? Categories { get; set; }
        public IReadOnlyCollection<Guid>? Tags { get; set; }
    }
}
