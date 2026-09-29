using ArticlesService.Domain.Enums;

namespace ArticlesService.Domain.Errors
{
    public static class CommentErrors
    {
        public static Error CommentNotFound(Guid id) => new(
            "NotFound", $"The comment with ID '{id}' was not found", ErrorType.NotFound);

        public static Error CommentsNotFound(IEnumerable<Guid> ids) => new (
            "NotFound", $"The following comments weren't found. IDs: {string.Join(Environment.NewLine, ids)}", ErrorType.NotFound);
               
        public static Error CommentDoesNotBelongToTheSpecifiedArticle(Guid articleId) => new (
           "BadRequest", $"The new comment does not belong to the specified article with ID '{articleId}'", ErrorType.BadRequest);

        public static Error ParentCommentNotFound(Guid parentCommentId) => new (
           "NotFound", $"Parent comment with ID {parentCommentId} was not found", ErrorType.NotFound);
    }
}
