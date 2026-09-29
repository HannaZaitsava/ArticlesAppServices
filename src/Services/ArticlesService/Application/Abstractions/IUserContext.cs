namespace ArticlesService.Application.Abstractions
{
    public interface IUserContext
    {
        bool IsAuthenticated { get; }
        Guid? UserId { get; }
        string? UserName { get; }
    }
}
