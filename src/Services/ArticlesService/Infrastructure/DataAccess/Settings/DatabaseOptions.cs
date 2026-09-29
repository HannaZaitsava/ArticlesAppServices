namespace ArticlesService.Infrastructure.DataAccess.Settings
{
    public sealed class DatabaseOptions
    {
        public const string SectionName = "ConnectionStrings";

        public string BaseDbConnection { get; set; } = string.Empty;
        public int MaxPoolSize { get; set; } = 100;
        public int MinPoolSize { get; set; } = 10;

        // Этот параметр скрыт в стандартном appsettings.json
        // Если его никто не передаст (Dev-ops, например), применится дефолт (500)
        public int MinPoolLimit { get; set; } = 1;
        public int MaxPoolLimit { get; set; } = 200;
    }
}
