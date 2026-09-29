namespace ArticlesService.Application.Common.Caching
{
    public static class CacheTags
    {
        public const string Articles = "articles-all";
        public static string Article(Guid id) => $"article:{id}";


        public const string ArticleCategories = "article-categories-all";
        public static string ArticleCategory(Guid id) => $"article-category:{id}";


        public const string Tags = "tags-all";
        public static string Tag(Guid id) => $"tag:{id}";


        public const string Comments = "comments-all";
        public static string Comment(Guid id) => $"comment:{id}";
        public static string ArticleComments(Guid articleId) => $"article-{articleId}-comments";
    }
}
