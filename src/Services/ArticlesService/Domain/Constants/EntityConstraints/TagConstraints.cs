namespace ArticlesService.Domain.Constants.EntityConstraints
{
    public static class TagConstraints
    {
        public const int MinLabelLength = 2;
        public const int MaxLabelLength = 50;

        public const int MaxColorLength = 7; // На случай конфигурации HasMaxLength(7) в EF Core
        public const string HexColorRegex = @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
    }
}
