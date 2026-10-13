namespace ArticlesService.Application.Common.CurrentUser
{
    public record CurrentUserInfo(Guid UserId, bool IsAuthenticated);
}
