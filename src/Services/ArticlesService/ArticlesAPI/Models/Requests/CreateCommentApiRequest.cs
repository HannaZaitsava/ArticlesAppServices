namespace ArticlesService.ArticlesAPI.Models.Requests
{
    public sealed record CreateCommentApiRequest(Guid? ParentId, string Text);    
}
