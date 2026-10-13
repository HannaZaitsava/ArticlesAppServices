namespace ArticlesService.Application.Common.CurrentUser
{
    public interface IUserScopedRequest
    {
        CurrentUserInfo UserContext { get; set; }
    }
}
