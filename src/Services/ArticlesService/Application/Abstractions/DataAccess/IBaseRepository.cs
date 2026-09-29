using System.Linq.Expressions;

namespace ArticlesService.Application.Abstractions.DataAccess
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        // ==============================================================================
        // ВЕТКА КОМАНД (Возвращают строго Entity) - методы для использования в командах
        // ==============================================================================
        Task<TEntity?> GetByIdAsync(Guid id, bool trackChanges = true, CancellationToken ct = default);
        Task<IEnumerable<TEntity>> GetListByIdsAsync(IEnumerable<Guid> ids, bool trackChanges = true, CancellationToken ct = default);
        Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? predicate = null, bool trackChanges = true, CancellationToken ct = default);
               
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
        void Update(TEntity entity);
        void UpdateRange(IEnumerable<TEntity> entities);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);

        Task<bool> IsExistingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

        Task ExecuteDeleteAsync(
            Expression<Func<TEntity, bool>> predicate,
            CancellationToken ct = default);      
        
        Task ExecuteUpdateAsync(
            Expression<Func<TEntity, bool>> predicate,
            Action<IUpdateSetter<TEntity>> updateAction,  // использует интерфейс-обертку, чтобы абстрагироваться от UpdateSettersBuilder EFCore
            CancellationToken cancellationToken = default);


        // =============================================================================
        // ВЕТКА ЗАПРОСОВ (Возвращают строго DTO) - методы для использования в запросах
        // =============================================================================
        Task<TDestination?> GetByIdProjectedAsync<TDestination>(Guid id, CancellationToken ct = default);
        Task<IEnumerable<TDestination>> GetListByIdsProjectedAsync<TDestination>(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task<IEnumerable<TDestination>> GetAllProjectedAsync<TDestination>(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);                 
    }
}
