namespace ArticlesService.ArticlesAPI.Models.Requests
{
    public sealed record ArticleApiRequest
    {
        public string? Title { get; init; } 
        public string? Content { get; init; } 

        //public IFormFile? CoverImage { get; set; }

        public IReadOnlyCollection<Guid>?  Categories { get; init; }
        public IReadOnlyCollection<Guid>? Tags { get; init; }
    }
}
