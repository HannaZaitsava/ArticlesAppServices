using ArticlesService.Application.DTOs.Articles;

namespace ArticlesService.Application.DTOs.Tags
{
    public sealed record TagResponseDTO()
    {
        public Guid Id { get; init; }
        public string Label { get; init; } = null!;

        public string? Color { get; init; }

        public IReadOnlyCollection<ArticleForTagResponseDTO> Articles { get; init; } = [];
    }
}
