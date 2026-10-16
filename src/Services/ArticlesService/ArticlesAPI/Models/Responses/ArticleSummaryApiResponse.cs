using ArticlesService.ArticlesAPI.Models.Enums;

namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record ArticleSummaryApiResponse
    {
        public Guid Id { get; init; }
        public ApiArticleStatus Status { get; init; }

        public string Title { get; init; } = null!;
        public DateTimeOffset? PublicationDate { get; init; } = default;

        public Guid AuthorId { get; init; } 
        public string AuthorName { get; init; } = default!;     
    }
}
