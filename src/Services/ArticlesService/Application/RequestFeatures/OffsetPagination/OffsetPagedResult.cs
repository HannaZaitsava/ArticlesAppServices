namespace ArticlesService.Application.RequestFeatures.OffsetPagination
{   
    public sealed record OffsetPagedResult<T>(
    int ItemsTotalCount,
    int PageIndex,
    int PageSize,
    IReadOnlyCollection<T> Items)
    {
        // Метод трансформации данных внутри пагинации
        public OffsetPagedResult<TResult> Map<TResult>(IReadOnlyCollection<TResult> newItems)
        {
            return new OffsetPagedResult<TResult>(ItemsTotalCount, PageIndex, PageSize, newItems);
        }
    }
}
