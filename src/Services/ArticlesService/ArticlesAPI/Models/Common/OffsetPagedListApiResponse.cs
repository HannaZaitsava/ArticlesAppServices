using System.ComponentModel;

namespace ArticlesService.ArticlesAPI.Models.Common
{
    public sealed record OffsetPagedListApiResponse<T>
    {
        [property: Description("The current zero-based page index (0 is the first page)")]
        public int PageIndex { get; init; }


        [property: Description("The maximum number of items returned on a single page")]
        public int PageSize { get; init; }


        [property: Description("The total number of items available across all pages in the database")]
        public int ItemsTotalCount { get; init; }


        [property: Description("The total number of calculated pages based on PageSize and ItemsTotalCount")]
        public int TotalPages => PageSize > 0
             ? (int)Math.Ceiling((double)ItemsTotalCount / PageSize)
             : 1;

        [property: Description("Indicates whether a previous page of results is available")]
        public bool HasPrevious => PageIndex > 0; //PageSize == 0 ? false : PageIndex > 1;


        [property: Description("Indicates whether a next page of results is available")]
        public bool HasNext => PageIndex < TotalPages - 1; // PageIndex == 0 ? false : PageIndex < TotalPages;


        [property: Description("The collection of data items for the current page")]
        public IReadOnlyCollection<T> Items { get; init; } = [];
    }
}
