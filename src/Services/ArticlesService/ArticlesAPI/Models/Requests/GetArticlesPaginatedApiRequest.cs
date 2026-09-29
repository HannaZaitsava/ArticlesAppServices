using ArticlesService.Application.RequestFeatures.Sorting;
using ArticlesService.ArticlesAPI.Models.Common;

namespace ArticlesService.ArticlesAPI.Models.Requests
{
    public sealed record GetArticlesPaginatedApiRequest(
        ArticleSortItem? Sorts
        //List<Sorting>? SortsNEW = null,
        //List<ArticleSortItem>? SortsNEW = null
        ) : OffsetPaginationApiRequest;
}
