namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record ArticleSummaryApiResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public DateTimeOffset? PublicationDate { get; init; } = default;

        public Guid CreatedBy { get; init; } 
        public string CreatorName { get; init; } = default!;     
    }
}
