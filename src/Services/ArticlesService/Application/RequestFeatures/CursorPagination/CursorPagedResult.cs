namespace ArticlesService.Application.RequestFeatures.CursorPagination;

/// <summary>
/// Результат курсор-основанной пагинации
/// </summary>
public record CursorPagedResult<T>(
    IReadOnlyCollection<T> Items,
    string? NextCursor,
    string? PreviousCursor,
    bool HasNextPage,
    bool HasPreviousPage,
    // int ItemsTotalCount,
    int PageSize)
{   
    // Метод трансформации данных внутри пагинации
    public CursorPagedResult<TResult> Map<TResult>(IReadOnlyCollection<TResult> newItems)
    {
        return new CursorPagedResult<TResult>(newItems, NextCursor, PreviousCursor, HasNextPage, HasPreviousPage, PageSize);
    }
}