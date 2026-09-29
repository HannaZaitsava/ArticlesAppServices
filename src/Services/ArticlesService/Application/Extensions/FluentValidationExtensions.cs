using System.Reflection;
using ArticlesService.Application.Abstractions;
using FluentValidation;

namespace ArticlesService.Application.Extensions
{
    public static class FluentValidationExtensions
    {        
        public static IRuleBuilderOptions<T, TSortItem?> IsValidSortItemForEntity<T, TSortItem, TEnum, TEntity>(
        this IRuleBuilder<T, TSortItem?> ruleBuilder)
        where TSortItem : ISortItem<TEnum>
        where TEnum : struct, Enum
        {
            return (IRuleBuilderOptions<T, TSortItem?>)ruleBuilder
                .Custom((sortItem, context) =>
                {
                    if (sortItem is null) return;
                    
                    string enumFieldName = sortItem.Field.ToString();

                    var propertyInEntity = typeof(TEntity).GetProperty(
                        enumFieldName,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                    );

                    if (propertyInEntity is null)
                    {
                        context.AddFailure(
                            context.PropertyPath,
                            $"The specified sort field value '{enumFieldName}' is invalid for {typeof(TEntity).Name}."
                        );
                    }
                });
        }
    }
}
