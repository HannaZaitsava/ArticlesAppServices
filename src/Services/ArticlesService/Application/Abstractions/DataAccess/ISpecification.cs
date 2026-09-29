using System.Linq.Expressions;

namespace ArticlesService.Application.Abstractions.DataAccess
{
    public interface ISpecification<TEntity>
    {   
        Expression<Func<TEntity, bool>>? Criteria { get; }                      
    }
}
