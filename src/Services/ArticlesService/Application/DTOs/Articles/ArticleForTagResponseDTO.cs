namespace ArticlesService.Application.DTOs.Articles
{
    public sealed record ArticleForTagResponseDTO
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = null!;
        public DateTimeOffset? PublicationDate { get; init; }
       
        public Guid CreatedBy { get; init; } 
        public string CreatorName { get; init; } = default!; 
    }
}
