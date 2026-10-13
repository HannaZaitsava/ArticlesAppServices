using ArticlesService.Domain.Enums;

namespace ArticlesService.Domain.Errors
{
    public static class TagErrors
    {
        public static Error TagNotFound(Guid id) => new Error(
           "Tags.NotFound", $"The tag with ID '{id}' was not found", ErrorType.NotFound);

        public static Error TagsNotFound(IEnumerable<Guid> ids) => new Error(
            "Tags.NotFound", $"The following tags weren't found. IDs: {string.Join(Environment.NewLine, ids)}", ErrorType.NotFound);

        public static Error TagAlreadyExists(string label) => new Error(
           "Tags.AlreadyExists", $"The tag with label '{label}' already exists", ErrorType.AlreadyExist);
    }
}
