using System.ComponentModel;

namespace ArticlesService.ArticlesAPI.Models.Common
{
    public sealed record CursorPagedListApiResponse<T>
    {
        [property: Description("Indicates whether a next page of results is available")]
        public bool HasNextPage { get; init; }

        [property: Description("The cursor pointing to the next page, or null if there are no more records ahead.")]
        public string? NextCursor { get; init; }


        [property: Description("Indicates whether a previous page of results is available")]
        public bool HasPreviousPage { get; init; }

        [property: Description("The cursor pointing to the previous page, or null if this is the first page.")]
        public string? PreviousCursor { get; init; }
        

        [property: Description("The maximum number of items returned on a single page")]
        public int PageSize { get; init; }

        //[property: Description("The total number of items available across all pages in the database")]
        //public int ItemsTotalCount { get; init; }

        [property: Description("The collection of data items for the current page")]
        public IReadOnlyCollection<T> Items { get; init; } = [];
    }
}