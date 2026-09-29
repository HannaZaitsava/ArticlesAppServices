using System.Security.Claims;

namespace ArticlesService.ArticlesAPI.Identity
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal principal)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier) // Для Cookies
             ?? principal.FindFirstValue("sub"); // Для JWT

            return Guid.TryParse(userId, out var parsedUserId) ? parsedUserId : null;
        }
    }
}
