using ArticlesService.Application.Abstractions;
using ArticlesService.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ArticlesService.Infrastructure.DataAccess.DbContext.Interceptors
{
    public class AuditAndSoftDeleteInterceptor : SaveChangesInterceptor
    {
        private readonly IUserContext? _userContext;
        private readonly TimeProvider _timeProvider;

        public AuditAndSoftDeleteInterceptor(TimeProvider timeProvider, IUserContext? userContext = null)
        {
            _userContext = userContext;
            _timeProvider = timeProvider;
        }

        public override InterceptionResult<int> SavingChanges(
          DbContextEventData eventData,
          InterceptionResult<int> result)
        {
            if (eventData.Context is not null)
            {
                UpdateAuditableAndSoftDeleteEntities(eventData.Context);
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
                UpdateAuditableAndSoftDeleteEntities(eventData.Context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditableAndSoftDeleteEntities(Microsoft.EntityFrameworkCore.DbContext context)
        {
            if (context is null) return;

            var now = _timeProvider.GetUtcNow();
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
                        if (auditable.CreatedOn == default)
                        {
                            auditable.CreatedOn = now;
                        }

                        // не заполнять принудительно, т.к. это поле может быть заполнено командой или фоновым воркером,
                        // который должен создавать статьи от имени конкретных авторов или системные статьи
                        if (auditable.CreatorId == Guid.Empty)
                        {
                            auditable.CreatorId = userId;
                        }
                    }

                    if (entry.State is EntityState.Added or EntityState.Modified)
                    {
                        if (auditable.LastModifiedOn == null || auditable.LastModifiedOn == default(DateTimeOffset))
                        {
                            auditable.LastModifiedOn = now;
                        }

                        // не заполнять принудительно, т.к. это поле может быть заполнено командой или фоновым воркером,
                        // который должен обновлять статьи от имени конкретных авторов или системные статьи
                        if (auditable.LastModifiedBy == null || auditable.LastModifiedBy == Guid.Empty)
                        {
                            auditable.LastModifiedBy = userId;
                        }
                    }
                }

                if (entry.Entity is ISoftDelete softDelete && entry.State == EntityState.Deleted)
                {
                    // Convert hard delete into soft delete
                    entry.State = EntityState.Modified;

                    //softDelete.IsDeleted = true;
                    if (softDelete.DeletedOn == null)
                    {
                        softDelete.DeletedOn = now;
                    }

                    if (softDelete.DeletedBy == null || softDelete.DeletedBy == Guid.Empty)
                    {
                        softDelete.DeletedBy = userId;
                    }

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
