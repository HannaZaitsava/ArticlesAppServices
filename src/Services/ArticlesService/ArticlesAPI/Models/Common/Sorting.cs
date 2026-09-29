namespace ArticlesService.ArticlesAPI.Models.Common
{ 
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    // Универсальный контракт для любого эндпоинта
    public record Sorting(string PropertyName, SortDirection Direction);
}
