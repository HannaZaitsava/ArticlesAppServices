using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Domain.Result;
using ArticlesService.Infrastructure.DataAccess.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ArticlesService.Infrastructure.DataAccess.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed;
        private IDbContextTransaction? _currentTransaction;
        public Guid? CurrentTransactionId => _currentTransaction?.TransactionId;//_context.Database.CurrentTransaction?.TransactionId;
        //public IDbContextTransaction GetCurrentTransaction() => _currentTransaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }       

        public int SaveChanges() => _context.SaveChanges();

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _context.SaveChangesAsync(cancellationToken);

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null)
                return;

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            try
            {
                await _context.SaveChangesAsync(ct);
                if (_currentTransaction != null) await _currentTransaction.CommitAsync(ct);
            }
            catch
            {
                await RollbackTransactionAsync(ct);
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                    _currentTransaction = null;
                }
            }
        }

        public async Task<TResponse> ExecuteInTransactionAsync<TResponse>(
        Func<Task<TResponse>> action,
        CancellationToken cancellationToken = default)
        {
            // Проверяем, нет ли уже активной транстанции
            if (_context.Database.CurrentTransaction != null)
            {
                return await action();
            }

            // Создаем стратегию повторов EF Core (от сетевых сбоев )
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await BeginTransactionAsync(cancellationToken);
                await using var transaction = _currentTransaction!;//await BeginTransactionAsync(cancellationToken);
                try
                {
                    var response = await action();

                    // Проверяем, не вернул ли хэндлер ошибку бизнес-логики (Result.Failure)
                    if (response is IResult result && !result.IsSuccess)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return response;
                    }

                    // Если всё успешно — коммитим в БД
                    await transaction.CommitAsync(cancellationToken);
                    return response;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
                return;

            await _currentTransaction.RollbackAsync(cancellationToken);
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _currentTransaction?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                
                if (_currentTransaction != null)
                {
                    await _currentTransaction.DisposeAsync();
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
