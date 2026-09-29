using System.Linq.Expressions;
using ArticlesService.Application.Abstractions.DataAccess;
using Microsoft.EntityFrameworkCore.Query;

namespace ArticlesService.Infrastructure.DataAccess.Repositories
{
    /// <summary>
    /// Адаптер, который связывает интерфейс с реальным UpdateSettersBuilder EF Core
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EFUpdateSetterAdapter<TEntity> : IUpdateSetter<TEntity>
    {
        private readonly UpdateSettersBuilder<TEntity> _builder;
        public EFUpdateSetterAdapter(UpdateSettersBuilder<TEntity> builder) => _builder = builder;

        public IUpdateSetter<TEntity> SetProperty<TValue>(
            Expression<Func<TEntity, TValue>> propertySelector,
            Expression<Func<TEntity, TValue>> valueSelector)
        {
            _builder.SetProperty(propertySelector, valueSelector);
            return this;
        }
    }
}
