using ArticlesService.Application.DTOs.Articles;

namespace ArticlesService.Application.DTOs.ArticleCategories
{
    public sealed record ArticleCategoryResponseDTO
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public IReadOnlyCollection<ArticleForTagResponseDTO> Articles { get; init; } = [];
    }
}
