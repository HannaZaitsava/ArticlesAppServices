using System.Linq.Expressions;
using ArticlesService.Domain.Exceptions;

namespace ArticlesService.Application.Helpers
{
    public static class ExpressionHelper
    {        
        public static Expression<Func<TEntity, object>> GetLambda<TEntity>(string propertyName)
        {
            var type = typeof(TEntity);
            var parameter = Expression.Parameter(typeof(TEntity), "x");

            var propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo is null)
            {
                throw new InvalidSortPropertyException(type.Name, propertyName);
            }

            var property = Expression.Property(parameter, propertyName);

            // Приводим к object, чтобы лямбда была универсальной (Func<T, object>)
            var conversion = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<TEntity, object>>(conversion, parameter);
        }
    }
}
