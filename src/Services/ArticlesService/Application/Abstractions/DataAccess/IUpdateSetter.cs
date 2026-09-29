using System.Linq.Expressions;

namespace ArticlesService.Application.Abstractions.DataAccess
{
    public interface IUpdateSetter<TEntity>
    {
        IUpdateSetter<TEntity> SetProperty<TValue>(
            Expression<Func<TEntity, TValue>> propertySelector,
            Expression<Func<TEntity, TValue>> valueSelector);
    }
}
