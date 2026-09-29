using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace ArticlesService.Infrastructure.DataAccess.Extensions
{
    public static class OffsetPaginationExrension
    {
        public static async Task<OffsetPagedResult<TDestination>> ToOffsetPagedListProjectedAsync<TSource, TDestination>(
         this IQueryable<TSource> query,
         OffsetPaginationParameters paginationParameters,
         IMapper mapper,
         CancellationToken cancellationToken = default)
        {
            var itemsTotalCount = await query.CountAsync(cancellationToken);

            int effectivePageSize = paginationParameters.PageSize == 0
                ? PaginationConstants.MaxPageSize
                : paginationParameters.PageSize;

            var items = await query
               .Paginate(paginationParameters.PageIndex, effectivePageSize)
               .ProjectToType<TSource, TDestination>(mapper)
               .ToListAsync(cancellationToken);

            return new OffsetPagedResult<TDestination>(
                itemsTotalCount,
                paginationParameters.PageIndex,
                effectivePageSize,
                items);
        }
    }
}
