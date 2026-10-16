using ArticlesService.ArticlesAPI.Models.Enums;

namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record ArticleApiResponse 
    {
        public Guid Id { get; init; }       
        public ApiArticleStatus Status { get; init; }

        public string Title { get; init; } = default!;
        public string Content { get; init; } = default!;
        public string PreviewText { get; init; } = default!;

        public DateTimeOffset? PublicationDate { get; init; }

        public DateTimeOffset CreatedOn { get; init; }       
        public Guid AuthorId { get; init; }
        public string AuthorName { get; init; } = default!;       

        public IReadOnlyCollection<ArticleCategoryShortInfoResponse> Categories { get; init; } = [];        
        public IReadOnlyCollection<TagShortInfoApiResponse> Tags { get; init; } = [];
    }
}
