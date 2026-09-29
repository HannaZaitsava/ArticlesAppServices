using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Application.DTOs.Tags;

namespace ArticlesService.Application.DTOs.Articles
{
    public sealed record ArticleShortInfoResponseDTO
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public string Content { get; init; } = null!;
        public DateTimeOffset? PublicationDate { get; init; }

        public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.Now;
        public Guid CreatedBy { get; init; }
        public string CreatorName { get; init; } = default!;

        public int CommentsTotalCount { get; init; }

        public IReadOnlyCollection<ArticleCategoryShotrInfoResponseDTO> Categories { get; init; } = [];        
        public IReadOnlyCollection<TagShortInfoResponseDTO> Tags { get; init; } = [];      
    }
}
