namespace ArticlesService.Application.Common.Caching
{
    public static class CacheKeys
    {
        public static string Article(Guid id) => $"article:{id}";
        public static string ArticleCategory(Guid id) => $"article-category:{id}";
        public static string Tag(Guid id) => $"tag:{id}";
        public static string Comment(Guid id) => $"comment:{id}";
    }
}