namespace ArticlesService.Application.DTOs.Articles
{
    public sealed record CreateArticleRequestDTO
    {
        public string Title { get; init; } = null!;
        public string Content { get; init; } = null!;       

        public IReadOnlyCollection<Guid>? CategoryIds { get; init; }
        public IReadOnlyCollection<Guid>? TagIds { get; init; }
    }
}
