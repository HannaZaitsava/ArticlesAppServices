using ArticlesService.Application.RequestFeatures.Sorting;
using ArticlesService.ArticlesAPI.Models.Common;
using ArticlesService.ArticlesAPI.Models.Enums;

namespace ArticlesService.ArticlesAPI.Models.Requests
{    
    public sealed record GetMyArticlesPaginatedApiRequest(       
       ArticleSortItem? Sorts,
       //List<Sorting>? SortsNEW = null,
       //List<ArticleSortItem>? SortsNEW = null
       ApiArticleStatus? Status = ApiArticleStatus.Published
       ) : OffsetPaginationApiRequest;
}
