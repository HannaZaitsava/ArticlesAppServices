namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record ArticleCategoryApiResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public IReadOnlyCollection<ArticleSummaryApiResponse> Articles { get; init; } = [];
    }
}
