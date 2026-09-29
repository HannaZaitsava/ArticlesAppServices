namespace ArticlesService.Domain.Entities.MtoM
{
    public sealed class ArticleTag
    {
        public Article Article { get; set; } = null!;
        public Guid ArticleId { get; set; }

        public Tag Tag { get; set; } = null!;
        public Guid TagId { get; set; }
    }
}
