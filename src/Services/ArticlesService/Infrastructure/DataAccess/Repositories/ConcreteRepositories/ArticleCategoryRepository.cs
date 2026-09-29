using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using ArticlesService.Infrastructure.DataAccess.Extensions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace ArticlesService.Infrastructure.DataAccess.Repositories.ConcreteRepositories
{
    public sealed class ArticleCategoryRepository : BaseRepository<ArticleCategory>, IArticleCategoryRepository
    {
        public ArticleCategoryRepository(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<ArticleCategory?> GetArticleCategoryWithFullInfoAsync(Guid id, bool trackChanges, CancellationToken ct)
        {
            return await _dbSet
                    .TrackChanges(trackChanges)
                    .Include(a => a.Articles)
                    .FirstOrDefaultAsync(a => a.Id == id, ct);
        }

        public async Task<OffsetPagedResult<TDestinationDTO>> GetOffsetPagedListProjectedAsync<TDestinationDTO>(OffsetPaginationParameters paginationParameters, CancellationToken ct = default)
        {
            return await _dbSet
                   .AsNoTracking()
                   .OrderBy(categoty => categoty.Name) 
                   .ToOffsetPagedListProjectedAsync<ArticleCategory, TDestinationDTO>(paginationParameters, _mapper, ct);
        }
    }
}
