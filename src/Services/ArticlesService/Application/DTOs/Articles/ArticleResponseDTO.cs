using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Domain.Enums;

namespace ArticlesService.Application.DTOs.Articles
{
    public sealed record ArticleResponseDTO
    {
        public Guid Id { get; init; }
        public ArticleStatus Status { get; init; }

        public string Title { get; init; } = default!;
        public string Content { get; init; } = default!;
        public string PreviewText { get; init; } = default!;

        public DateTimeOffset? PublicationDate { get; init; }

        public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.Now;
        public Guid AuthorId { get; init; }
        public string AuthorName { get; init; } = default!; 

        public IReadOnlyCollection<ArticleCategoryShotrInfoResponseDTO> Categories { get; init; } = [];
        public IReadOnlyCollection<TagShortInfoResponseDTO> Tags { get; init; } = [];
    }
}
