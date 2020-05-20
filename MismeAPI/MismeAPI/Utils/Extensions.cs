using System.Security.Claims;

namespace MismeAPI.Utils
{
    public static class Extensions
    {
        public static string ToForgotPasswordEmail(this string resource, string password)
        {
            return resource.Replace("#PASSWORD#", password);
        }

        public static string ToActivationAccountEmail(this string resource, string code)
        {
            return resource.Replace("#CODE#", code);
        }

        public static string ToSendInvitationEmail(this string resource, string fullname, string downloadLink)
        {
            return resource.Replace("#FULLNAME#", fullname).Replace("#APKLINK#", downloadLink);
        }

        public static int GetUserIdFromToken(this ClaimsPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            return int.Parse(userId);
        }
    }
}
