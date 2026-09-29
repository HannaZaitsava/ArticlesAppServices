using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Domain.Entities;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace ArticlesService.Infrastructure.DataAccess.Repositories.ConcreteRepositories
{
    public sealed class CommentRepository : BaseRepository<Comment>, ICommentRepository
    {
        public CommentRepository(AppDbContext context, IMapper mapper) : base(context, mapper)
        {
        }       

        public async Task<IList<TDestinationDTO>> GetNestedCommentsProjectedAsync<TDestinationDTO>(
           IEnumerable<Guid> rootCommentsIds,           
           CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .IgnoreQueryFilters() // чтобы сохранить хронологию soft delete, если она используется
                .Where(c => c.RootCommentId != null && rootCommentsIds.Contains(c.RootCommentId.Value))
                .ProjectToType<TDestinationDTO>(_mapper.Config)
                .ToListAsync(cancellationToken);
        }      
    }
}
