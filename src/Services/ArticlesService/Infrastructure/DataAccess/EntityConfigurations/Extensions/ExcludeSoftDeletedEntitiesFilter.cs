using System.Linq.Expressions;
using ArticlesService.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ArticlesService.Infrastructure.DataAccess.EntityConfigurations.Extensions
{
    public static class ExcludeSoftDeletedEntitiesFilter
    {
        public static void ApplyExcludeSoftDeletedEntitiesFilter(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(BuildExcludeSoftDeletedEntitiesFilter(entityType));
                }
            }
        }

        /// <summary>
        /// Method to create e => !e.IsDeleted expression
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private static LambdaExpression BuildExcludeSoftDeletedEntitiesFilter(IMutableEntityType entityType)
        {
            var parameter = Expression.Parameter(entityType.ClrType, "e");

            var condition = Expression.Equal(
                Expression.Property(parameter, nameof(ISoftDelete.DeletedOn)),
                Expression.Constant(null, typeof(DateTimeOffset?)));

            return Expression.Lambda(condition, parameter);
        }
    }
}
