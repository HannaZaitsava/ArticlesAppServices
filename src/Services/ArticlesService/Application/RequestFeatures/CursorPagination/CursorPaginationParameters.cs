using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.Enums;

namespace ArticlesService.Application.RequestFeatures.CursorPagination
{
    public record CursorPaginationParameters(
     string? Cursor = null,
     int PageSize = PaginationConstants.DefaultPageSize,
     PaginationDirection Direction = PaginationDirection.Forward)
    {
    }
}
