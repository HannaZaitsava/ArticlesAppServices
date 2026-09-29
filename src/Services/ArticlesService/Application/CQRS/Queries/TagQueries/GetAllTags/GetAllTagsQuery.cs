using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.TagQueries.GetAllTags
{
    public sealed record GetAllTagsQuery() :
        IRequest<Result<OffsetPagedResult<TagShortInfoResponseDTO>>>,
        ICachableRequest
    {        
        public OffsetPaginationParameters PaginationParameters { get; init; } = new()
        {
            PageIndex = PaginationConstants.MinPageIndex,
            PageSize = PaginationConstants.TagsDefaultPageSize,
        };

        public string GetCacheKeyMetadata() =>
            $"p:{PaginationParameters.PageIndex}:s:{PaginationParameters.PageSize}";
        //$"q:{Search?.Trim().ToLowerInvariant()}:" +
        //    $"sort:{Sorts?.Field}:{Sorts?.IsDescending}";
        // $"sorts:{string.Join(",", Sorts.Select(s => $"{s.Field}_{s.IsDescending}"))}" // для будущего списка сортировок
        // Если перейти на IEnumerable<ArticleSortItem>, перед генерацией метаданных обязательно нужно сортировать
        // сам список сортировок(например, по имени поля). Это предотвратит создание разных ключей для Title, Date и Date, Title,
        // если они выдают одинаковый результат в БД.

        public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.Tags];
    }
}
