using ArticlesService.Domain.Enums;

namespace ArticlesService.Domain.Errors
{
    public static class ArticleErrors
    {
        public static Error ArticleNotFound(Guid id) => new Error(
            "NotFound", $"Article with ID '{id}' was not found", ErrorType.NotFound);

        public static Error ArticlesNotFound(IEnumerable<Guid> ids) => new Error(
            "NotFound", $"The following articles weren't found. IDs: {string.Join(Environment.NewLine, ids)}", ErrorType.NotFound);

        public static Error ArticleAlreadyExists(string title) => new Error(
            "AlreadyExists", $"Article with title '{title}' already exists", ErrorType.AlreadyExist);

        public static Error ArticleAlreadyPublished(Guid id) => new Error(
           "AlreadyPublished", $"Article with ID '{id}' already published", ErrorType.BadRequest);
    }
}
