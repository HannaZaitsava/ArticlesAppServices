using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.DTOs.ArticleCategories;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.ArticleCategoryQueries.GetAllArticleCategories
{          
    public sealed record GetAllArticleCategoriesQuery :
        IRequest<Result<OffsetPagedResult<ArticleCategoryShotrInfoResponseDTO>>>,
        ICachableRequest
    {        
        public OffsetPaginationParameters PaginationParameters { get; init; } = new()
        {
            PageIndex = PaginationConstants.MinPageIndex,
            PageSize = PaginationConstants.ArticleCategoriesDefaultPageSize,
        };
        
        public string GetCacheKeyMetadata() =>
            $"p:{PaginationParameters.PageIndex}:s:{PaginationParameters.PageSize}";
            //$"q:{Search?.Trim().ToLowerInvariant()}:" +
        //    $"sort:{Sorts?.Field}:{Sorts?.IsDescending}";
        // $"sorts:{string.Join(",", Sorts.Select(s => $"{s.Field}_{s.IsDescending}"))}" // для будущего списка сортировок
        // Если перейти на IEnumerable<ArticleSortItem>, перед генерацией метаданных обязательно нужно сортировать
        // сам список сортировок(например, по имени поля). Это предотвратит создание разных ключей для Title, Date и Date, Title,
        // если они выдают одинаковый результат в БД.

        public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.ArticleCategories];
    }
}
