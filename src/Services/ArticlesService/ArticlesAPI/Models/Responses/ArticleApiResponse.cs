namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record ArticleApiResponse 
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Content { get; init; } = default!;
        public DateTimeOffset? PublicationDate { get; init; }

        public DateTimeOffset CreatedOn { get; init; } = DateTimeOffset.UtcNow;       
        public Guid CreatedBy { get; init; }
        public string CreatorName { get; init; } = default!;       

        public IReadOnlyCollection<ArticleCategoryShortInfoResponse> Categories { get; init; } = [];        
        public IReadOnlyCollection<TagShortInfoApiResponse> Tags { get; init; } = [];
    }
}
