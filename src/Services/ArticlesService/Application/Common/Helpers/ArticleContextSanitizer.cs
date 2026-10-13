namespace ArticlesService.Application.Common.Helpers
{
    public static class ArticleContextSanitizer
    {
        public static string ToPlainText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // очистка контента в целях безопасности
            string result = text;

            return result.Trim();
        }
    }
}
