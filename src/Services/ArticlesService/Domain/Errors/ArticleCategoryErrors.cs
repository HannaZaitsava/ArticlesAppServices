using ArticlesService.Domain.Enums;

namespace ArticlesService.Domain.Errors
{
    public static class ArticleCategoryErrors
    {
        public static Error ArticleCategoryNotFound(Guid id) => new Error(
            "NotFound", $"The article category with ID '{id}' was not found", ErrorType.NotFound);

        public static Error ArticleCategoriesNotFound(IEnumerable<Guid> ids) => new Error(
            "NotFound", $"The following article categories weren't found. IDs: {string.Join(Environment.NewLine, ids)}", ErrorType.NotFound);

        public static Error ArticleCategoryAlreadyExists(string name) => new Error(
            "AlreadyExists", $"The article category with name '{name}' already exists", ErrorType.AlreadyExist);
    }
}
