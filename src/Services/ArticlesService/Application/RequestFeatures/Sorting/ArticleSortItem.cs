using ArticlesService.Application.Abstractions;
using ArticlesService.Application.Enums.SortingEnums;

namespace ArticlesService.Application.RequestFeatures.Sorting
{
    public class ArticleSortItem : ISortItem<ArticleSortField>
    {
        public ArticleSortField Field { get; set; }
        public bool IsDescending { get; set; } = false;        
    }
}
