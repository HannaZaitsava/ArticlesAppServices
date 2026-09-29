using ArticlesService.Application.Common.Caching;
using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.DTOs.Comments;
using ArticlesService.Application.RequestFeatures.OffsetPagination;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Queries.CommentQueries.GetCommentsOffsetPagedQuery
{
	public sealed record GetCommentsOffsetPagedQuery :		
		IRequest<Result<OffsetPagedResult<CommentResponseDTO>>>,
        ICachableRequest
    {
		public Guid ArticleId { get; init; }

        public OffsetPaginationParameters PaginationParameters { get; init; } = new()
        {
            PageIndex = PaginationConstants.MinPageIndex,
            PageSize = PaginationConstants.CommentsDefaultPageSize,
        };      
		public string GetCacheKeyMetadata() =>
			$"article:{ArticleId}:p:{PaginationParameters.PageIndex}:s:{PaginationParameters.PageSize}:";

		//$"q:{Search?.Trim().ToLowerInvariant()}:" + // .Trim().ToLowerInvariant() - сделать раньше, чем будет вызван handler
		//   $"sort:{Sorts?.Field}:{Sorts?.IsDescending}";
		// $"sorts:{string.Join(",", Sorts.Select(s => $"{s.Field}_{s.IsDescending}"))}" // для будущего списка сортировок
		// Если перейти на IEnumerable<ArticleSortItem>, перед генерацией метаданных обязательно нужно сортировать
		// сам список сортировок(например, по имени поля). Это предотвратит создание разных ключей для Title, Date и Date, Title,
		// если они выдают одинаковый результат в БД.

		public IEnumerable<string>? CacheTags => [Common.Caching.CacheTags.ArticleComments(ArticleId), Common.Caching.CacheTags.Comments];
	}
}
