using System.ComponentModel;
using ArticlesService.Application.Enums;

namespace ArticlesService.ArticlesAPI.Models.Common
{
    public record CursorPaginationApiRequest(
    [property: Description("Number of items to return in a single page. If null, system default is used.")]
     int PageSize = 0,

    [property: Description("The cursor to start pagination from. If null, system default is used.")]
     string? Cursor = null,

    [property: Description("The direction of pagination. If null, system default is used.")]
      PaginationDirection Direction = PaginationDirection.Forward
    );
}
