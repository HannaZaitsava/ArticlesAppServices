using System.Linq.Expressions;
using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Domain.Entities.Base;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using ArticlesService.Infrastructure.DataAccess.Extensions;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace ArticlesService.Infrastructure.DataAccess.Repositories
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
    {
        protected readonly AppDbContext _context = default!;
        protected readonly DbSet<TEntity> _dbSet = default!;
        protected readonly IMapper _mapper;        

        public BaseRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<bool> IsExistingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        //public virtual async Task<bool> AnyAsync(ISpecification<TEntity>? spec, CancellationToken cancellationToken = default)
        //{
        //    // Используем стандартный Evaluator для применения фильтров спецификации
        //    var query = SpecificationEvaluator<TEntity>.GetBaseQuery(_dbSet.AsQueryable(), spec);
        //    return await query.AnyAsync(cancellationToken);
        //}

        public virtual async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(cancellationToken);
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }

        // ==============================================================================
        // ВЕТКА КОМАНД (Возвращают строго Entity) - методы для использования в командах
        // ==============================================================================

        public virtual async Task<TEntity?> GetByIdAsync(Guid id, bool trackChanges = true, CancellationToken ct = default)
        {
            IQueryable<TEntity> query = _dbSet;
            if (!trackChanges) query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<IEnumerable<TEntity>> GetListByIdsAsync(
            IEnumerable<Guid> ids,
            bool trackChanges = true,
            CancellationToken ct = default)
        {
            return await GetAllAsync(e => ids.Contains(e.Id), trackChanges, ct);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool trackChanges = true,
            CancellationToken ct = default)
        {            
            return await _dbSet.TrackChanges(trackChanges).FilterByExpression(predicate).ToListAsync(ct);
        }

        public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }


        // Bulk operations
        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public virtual async Task ExecuteDeleteAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken cancellationToken = default)
        {
            await _dbSet.Where(predicate).ExecuteDeleteAsync(cancellationToken);
        }

        public virtual async Task ExecuteUpdateAsync(
            Expression<Func<TEntity, bool>> predicate,
            Action<IUpdateSetter<TEntity>> updateAction,
            CancellationToken cancellationToken = default)
        {
            await _context.Set<TEntity>()
                .Where(predicate)
                .ExecuteUpdateAsync(builder => updateAction(new EFUpdateSetterAdapter<TEntity>(builder)), cancellationToken); // Адаптер, который связывает интерфейс с реальным UpdateSettersBuilder EF Core
        }


        // =============================================================================
        // ВЕТКА ЗАПРОСОВ (Возвращают строго DTO) - методы для использования в запросах
        // =============================================================================
        public virtual async Task<TDestination?> GetByIdProjectedAsync<TDestination>(Guid id, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(e => e.Id == id)
                .ProjectToType<TDestination>(_mapper.Config)
                .FirstOrDefaultAsync(ct);
        }

        public virtual async Task<IEnumerable<TDestination>> GetListByIdsProjectedAsync<TDestination>(
            IEnumerable<Guid> ids,
            CancellationToken ct = default)
        {
            return await GetAllProjectedAsync<TDestination>(e => ids.Contains(e.Id), ct);
        }

        public virtual async Task<IEnumerable<TDestination>> GetAllProjectedAsync<TDestination>(
            Expression<Func<TEntity, bool>>? predicate = null,
            CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FilterByExpression(predicate)
                .ProjectToType<TDestination>(_mapper.Config) 
                .ToListAsync(ct);
        }  
    }
}
