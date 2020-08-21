using Microsoft.AspNetCore.Http;

namespace MismeAPI.Utils
{
    public static class HttpContextExtension
    {
        /// <summary>
        /// Get current user id from claim principal
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <returns></returns>
        public static int CurrentUser(this IHttpContextAccessor httpContextAccessor)
        {
            var id = httpContextAccessor?.HttpContext?.User?.GetUserIdFromToken();

            return id.HasValue ? id.Value : 0;
        }
    }
}
