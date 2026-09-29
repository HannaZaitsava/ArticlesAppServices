namespace ArticlesService.Application.Abstractions.DataAccess
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        Guid? CurrentTransactionId { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
        Task<TResponse> ExecuteInTransactionAsync<TResponse>(Func<Task<TResponse>> action, CancellationToken cancellationToken = default);
    }
}
