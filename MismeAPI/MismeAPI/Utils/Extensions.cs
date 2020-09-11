using System;
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

        public static string ToSendInvitationEmail(this string resource, string fullname, string iOSLink, string androidLink)
        {
            return resource.Replace("#FULLNAME#", fullname).Replace("#APKLINKIOS#", iOSLink).Replace("#APKLINKANDROID#", androidLink);
        }

        public static int GetUserIdFromToken(this ClaimsPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            return int.Parse(userId);
        }

        public static DateTime FromUTCToLocalTime(this DateTime dt, int timezoneOffset)
        {
            // Convert a DateTime from UTC timezone into the user's timezone.

            return dt.AddHours(timezoneOffset);
        }
    }
}
