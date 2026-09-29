namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record TagApiResponse
    {
        public Guid Id { get; init; }
        public string Label { get; init; } = null!;

        public string? Color { get; init; }

        public IReadOnlyCollection<ArticleSummaryApiResponse> Articles { get; init; } = [];
    }
}
