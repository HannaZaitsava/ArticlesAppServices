using System.Security.Claims;
using ArticlesService.Application.Abstractions;

namespace ArticlesService.ArticlesAPI.Identity
{
    public sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        public Guid? UserId => User?.GetUserId();

        public string? UserName => User?.FindFirst(ClaimTypes.Name)?.Value;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    }
}
