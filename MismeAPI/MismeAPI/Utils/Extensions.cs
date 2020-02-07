using System.Security.Claims;

namespace MismeAPI.Utils
{
    public static class Extensions
    {
        public static string ToForgotPasswordEmail(this string resource, string password)
        {
            return resource.Replace("#PASSWORD#", password);
        }

        public static int GetUserIdFromToken(this ClaimsPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            return int.Parse(userId);
        }
    }
}