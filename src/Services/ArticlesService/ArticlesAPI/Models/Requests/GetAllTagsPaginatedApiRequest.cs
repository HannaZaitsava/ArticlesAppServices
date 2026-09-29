using ArticlesService.ArticlesAPI.Models.Common;

namespace ArticlesService.ArticlesAPI.Models.Requests
{
    public sealed record GetAllTagsPaginatedApiRequest: OffsetPaginationApiRequest
    {
    }
}
