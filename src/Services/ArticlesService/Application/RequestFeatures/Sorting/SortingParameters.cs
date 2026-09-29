using System.Linq.Expressions;
using ArticlesService.Application.Abstractions;
using ArticlesService.Application.Helpers;

namespace ArticlesService.Application.RequestFeatures.Sorting
{   
    public class SortingParameters<TEntity, TSortItem, TEnum>
    where TEnum : struct, Enum
    where TSortItem : ISortItem<TEnum>
    {
       public List<(LambdaExpression KeySelector, bool IsDescending)> SortOrders { get; } = [];

        public SortingParameters(TSortItem sort)
        {
            ApplySortingFromParams(sort);
        }
       private void ApplySortingFromParams(TSortItem? sort)
        {
            if (sort is null) return;

            string propertyName = sort.Field.ToString();

            var selector = ExpressionHelper.GetLambda<TEntity>(propertyName);

            AddSort(selector, sort.IsDescending);
        }
        protected void AddSort(LambdaExpression keySelector, bool isDescending = false)
        {
            SortOrders.Add((keySelector, isDescending));
        }

        // Перегрузка для вызовов такого вида: sortParams.AddSort(article => article.Id)
        protected void AddSort<TKey>(Expression<Func<TEntity, TKey>> keySelector, bool isDescending = false)
        {
            SortOrders.Add((keySelector, isDescending));
        }
    }
}
