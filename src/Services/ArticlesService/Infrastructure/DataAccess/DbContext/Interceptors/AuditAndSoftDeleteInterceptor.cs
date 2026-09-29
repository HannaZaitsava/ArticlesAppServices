using ArticlesService.Application.Abstractions;
using ArticlesService.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ArticlesService.Infrastructure.DataAccess.DbContext.Interceptors
{
    public class AuditAndSoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly IUserContext? _userContext; 

        public AuditAndSoftDeleteInterceptor(IUserContext? userContext = null) => _userContext = userContext;

        public override InterceptionResult<int> SavingChanges(
          DbContextEventData eventData,
          InterceptionResult<int> result)
        {           
            if (eventData.Context is not null)
            {
                UpdateAuditableAndSodtDeleteEntities(eventData.Context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {      
            if (eventData.Context is not null)
            {
                UpdateAuditableAndSodtDeleteEntities(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableAndSodtDeleteEntities(Microsoft.EntityFrameworkCore.DbContext context)
        {
            if (context is null) return;

            var now = DateTimeOffset.UtcNow;
            var userId = _userContext?.UserId ?? Guid.Empty;

            var entries = context.ChangeTracker.Entries()
               .Where(e => e.Entity is IAuditableEntity or ISoftDelete)
               .Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditableEntity auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedOn = now;
                        auditable.CreatedBy = userId;
                    }

                    if (entry.State is EntityState.Added or EntityState.Modified)
                    {
                        auditable.LastModifiedOn = now;
                        auditable.LastModifiedBy = userId;
                    }
                }

                if (entry.Entity is ISoftDelete softDelete && entry.State == EntityState.Deleted)
                {
                    // Convert hard delete into soft delete
                    entry.State = EntityState.Modified;

                    //softDelete.IsDeleted = true;
                    softDelete.DeletedOn = now;
                    softDelete.DeletedBy = userId;

                    // Also counts as an update
                    if (entry.Entity is IAuditableEntity a)
                    {
                        a.LastModifiedOn = now;
                        a.LastModifiedBy = userId;
                    }
                }
            }
        }
    }
}
