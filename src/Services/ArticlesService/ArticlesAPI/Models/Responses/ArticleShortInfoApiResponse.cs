namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record ArticleShortInfoApiResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public string Content { get; init; } = null!;
        public DateTimeOffset PublicationDate { get; init; } = DateTimeOffset.UtcNow;

        public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.UtcNow;
        public Guid CreatedBy { get; init; }
        public string CreatorName { get; init; } = default!;

        public int CommentsTotalCount { get; init; }

        public IReadOnlyCollection<ArticleCategoryShortInfoResponse> Categories { get; init; } = [];
        public IReadOnlyCollection<TagShortInfoApiResponse> Tags { get; init; } = [];        
    }
}
