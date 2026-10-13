using ArticlesService.Domain.Enums;

namespace ArticlesService.Domain.Errors
{
    public static class ArticleErrors
    {
        public static Error ArticleNotFound(Guid id) => new Error(
            "Article.NotFound", $"Article with ID '{id}' was not found", ErrorType.NotFound);

        public static Error ArticlesNotFound(IEnumerable<Guid> ids) => new Error(
            "Article.NotFound", $"The following articles weren't found. IDs: {string.Join(Environment.NewLine, ids)}", ErrorType.NotFound);

        public static Error ArticleAlreadyExists(string title) => new Error(
            "Article.AlreadyExists", $"Article with title '{title}' already exists", ErrorType.AlreadyExist);

        public static Error ArticleAlreadyPublished(Guid id) => new Error(
            "Article.AlreadyPublished", $"Article with ID '{id}' already published", ErrorType.BadRequest);

        public static Error EmptyContentCannotBePublished(Guid id) => new Error(
             Name: "Article.EmptyContent",
             Message: $"Cannot publish article '{id}' because it has no content. Please add content before publishing.",
             Type: ErrorType.BadRequest);
    }
}
