using System.ComponentModel;

namespace ArticlesService.ArticlesAPI.Models.Common
{   
    public record OffsetPaginationApiRequest(
    [property: Description("The index of the page to return. If null, system default is used.")]
    int? PageIndex = null,

    [property: Description("Number of items to return in a single page. If null, system default is used.")]
    int? PageSize = null   
    );
}
