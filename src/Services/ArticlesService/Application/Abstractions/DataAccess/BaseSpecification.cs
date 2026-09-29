using System.Linq.Expressions;

namespace ArticlesService.Application.Abstractions.DataAccess
{
    public abstract class BaseSpecification<TEntity> : ISpecification<TEntity>
    {        
        protected BaseSpecification(Expression<Func<TEntity, bool>>? criteria = null) => Criteria = criteria;

        public Expression<Func<TEntity, bool>>? Criteria { get; }       
    }
}
