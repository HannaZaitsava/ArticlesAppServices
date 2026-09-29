namespace ArticlesService.Application.DTOs.Comments
{
    //public readonly record struct ParentCommentInfoDTO(Guid Id, Guid? RootCommentId, Guid ArticleId);
    //public record struct ParentCommentInfoDTO(Guid Id, Guid? RootCommentId, Guid ArticleId);
    public sealed record ParentCommentInfoDTO(Guid Id, Guid? RootCommentId, Guid ArticleId);
}
