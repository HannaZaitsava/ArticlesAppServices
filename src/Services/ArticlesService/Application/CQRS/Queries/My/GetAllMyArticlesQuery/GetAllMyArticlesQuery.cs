using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.Common.CurrentUser;
using ArticlesService.Application.DTOs.Articles;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Application.RequestFeatures.Sorting;
using ArticlesService.Domain.Enums;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.My.GetAllMyArticlesQuery
{
    public sealed record GetAllMyArticlesQuery :
        IRequest<Result<OffsetPagedResult<ArticleShortInfoResponseDTO>>>,
        ICachableRequest,
        IUserScopedRequest
    {
        public CurrentUserInfo UserContext { get; set; } = null!;
        public ArticleStatus Status { get; init; } = ArticleStatus.Published;
        public ArticleSortItem? Sorts { get; init; } = null;

        public OffsetPaginationParameters PaginationParameters { get; init; } = new()
        {
            PageIndex = PaginationConstants.MinPageIndex,
            PageSize = PaginationConstants.ArticlesDefaultPageSize,
        };

        public string GetCacheKeyMetadata()
        {
            if (UserContext is not { IsAuthenticated: true })
            {
                throw new InvalidOperationException($"Cannot generate cache key for {nameof(GetAllMyArticlesQuery)}: User is not authenticated.");
            }

            var userPart = $"usr:{UserContext.UserId}";
            var filterPart = $"status:{(int)Status}";
            var paginationPart = $"p:{PaginationParameters.PageIndex}:s:{PaginationParameters.PageSize}";
            //var searchPart = $"q:{Search?.Trim().ToLowerInvariant()}:";
            var sortPart = Sorts is not null
                ? $"sort:{Sorts.Field}_{Sorts.IsDescending}"
                : "sort:default";

            /* 
            // Когда перейду на IEnumerable<ArticleSortItem>.
            // Сортируем параметры сортировки по алфавиту названия поля.
            // Это гарантирует детерминированность ключа, независимо от того, как фронтенд передал массив (в каком поядке пришли параметры сортировки)
            var sortPart = Sorts is not null && Sorts.Any()
                ? "sort:" + string.Join(",", Sorts.OrderBy(s => s.Field).Select(s => $"{s.Field}_{s.IsDescending}"))
                : "sort:default";
            */

            return $"{userPart}:{filterPart}:{paginationPart}:{sortPart}";
        }            
    }
}
