using ArticlesService.Application.Abstractions.DataAccess;
using ArticlesService.Domain.Entities;
using ArticlesService.Domain.Enums;

namespace ArticlesService.Application.Specifications
{
    public class ArticleIsPublishedSpec : BaseSpecification<Article>
    {    
        public ArticleIsPublishedSpec(Guid id) : base(a => a.Id == id && a.Status == ArticleStatus.Published)
        {
        }        
    }
}
