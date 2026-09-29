namespace ArticlesService.Infrastructure.DataAccess.Pagination
{
    public record struct CreatedOnCursor(DateTimeOffset CreatedOn, Guid Id);
}
