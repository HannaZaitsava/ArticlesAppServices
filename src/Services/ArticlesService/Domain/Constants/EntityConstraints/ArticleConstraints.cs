namespace ArticlesService.Domain.Constants.EntityConstraints
{
    public static class ArticleConstraints
    {
        public const int TitleMaxLength = 255;
        public const int TitleMinLength = 3;

        public const int ContentMaxLength = 1000;
        public const int ContentMinLength = 10;
    }
}
