using System.Linq.Expressions;
using ArticlesService.Application.Abstractions;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.Helpers;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace ArticlesService.Infrastructure.DataAccess.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<TEntity> FilterByExpression<TEntity>(
            this IQueryable<TEntity> entities,
            Expression<Func<TEntity, bool>>? filterExpression)
            where TEntity : class
        {
            return filterExpression is null ? entities : entities.Where(filterExpression);
        }      

        public static IQueryable<TEntity> TrackChanges<TEntity>(
            this IQueryable<TEntity> query,
            bool trackChanges)
            where TEntity : class
        {
            return trackChanges ? query : query.AsNoTracking();
        }

        public static IQueryable<TDestination> ProjectToType<TEntity, TDestination>(
            this IQueryable<TEntity> query,
            IMapper mapper)
        {           
            return typeof(TDestination) == typeof(TEntity)
                    ? (IQueryable<TDestination>)query
                    : query.ProjectToType<TDestination>(mapper.Config);
        }

        public static IQueryable<TEntity> Paginate<TEntity>(
            this IQueryable<TEntity> query,
            int pageIndex,
            int pageSize            
        )
        {            
            // Если pageSize равен 0, мы отключаем пагинацию, но ставим "предохранитель" (например, максимум 1000 строк)
            if (pageSize == 0)
            {
                return query.Take(PaginationConstants.MaxPageSize);
            }

            // Если используется zero-based pagination => использовать pageIndex - НАШ СЛУЧАЙ
            // Если используется nonzero-page pagination => использовать (pageIndex - 1) 
            return query              
                .Skip(pageIndex * pageSize)
                .Take(pageSize);
        }        

        public static IEnumerable<(LambdaExpression KeySelector, bool IsDescending)> ToSortingExpressions<TEntity, TEnum>(
        this ISortItem<TEnum>? sort)
        where TEnum : struct, Enum
        {
            if (sort is null)
                return []; 

            string propertyName = sort.Field.ToString();
            var selector = ExpressionHelper.GetLambda<TEntity>(propertyName);

            return [(selector, sort.IsDescending)];
        }

        public static IQueryable<TEntity> ApplySortOrders<TEntity>(
         this IQueryable<TEntity> query,
         IEnumerable<(LambdaExpression KeySelector, bool IsDescending)> sortOrders)
        {
            bool isFirst = true;

            foreach (var order in sortOrders)
            {
                // Используем nameof вместо "OrderBy", "ThenBy" и т.д.
                // Теперь компилятор сам подставит правильные строки, а при рефакторинге код не сломается.
                string methodName = isFirst
                    ? (order.IsDescending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                    : (order.IsDescending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

                isFirst = false;

                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    new Type[] { typeof(TEntity), order.KeySelector.ReturnType },
                    query.Expression,
                    Expression.Quote(order.KeySelector)
                );

                query = query.Provider.CreateQuery<TEntity>(resultExpression);
            }

            return query;
        }

        public static IQueryable<TEntity> ApplySorting<TEntity, TKey>(
        this IQueryable<TEntity> query,
        IEnumerable<(Expression<Func<TEntity, TKey>> KeySelector, bool IsDescending)> sortOrders)
        {
            IOrderedQueryable<TEntity> orderedQuery = null!;

            foreach (var sort in sortOrders)
            {
                if (orderedQuery is null)
                {
                    // Первый элемент — инициализируем OrderBy
                    orderedQuery = sort.IsDescending
                        ? query.OrderByDescending(sort.KeySelector)
                        : query.OrderBy(sort.KeySelector);
                }
                else
                {
                    // Последующие элементы — добавляем ThenBy
                    orderedQuery = sort.IsDescending
                        ? orderedQuery.ThenByDescending(sort.KeySelector)
                        : orderedQuery.ThenBy(sort.KeySelector);
                }
            }

            return orderedQuery ?? query;
        }            
    }
}
