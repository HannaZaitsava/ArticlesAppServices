using ArticlesService.Domain.Entities;

namespace ArticlesService.Domain.Entities.MtoM
{   
    public sealed class ArticleArticleCategory
    {
        public Article Article { get; set; } = null!;
        public Guid ArticleId { get; set; }

        public ArticleCategory Category { get; set; } = null!;
        public Guid CategoryId { get; set; }
    }
}
