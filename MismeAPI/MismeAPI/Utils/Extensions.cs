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

        public static string ToManualEmail(this string resource, string subject, string body)
        {
            return resource.Replace("#SUBJECT#", subject).Replace("#BODY#", body);
        }

        public static string ToHeaderEmail(this string resource, string subject)
        {
            return resource.Replace("#HEADERSUBJECT#", subject);
        }

        public static string SetHeaderFooterToTemplate(this string resource, string header, string footer)
        {
            return resource.Replace("#HEADER#", header).Replace("#FOOTER#", footer);
        }

        public static string ToGroupAdminInviteEmail(this string resource, bool isNewUser, string adminUrl, string email = "", string password = "")
        {
            if (isNewUser)
                return resource.Replace("#EMAIL#", email).Replace("#PASSWORD#", password).Replace("#AMINURL#", adminUrl);
            else
                return resource.Replace("#AMINURL#", adminUrl);
        }

        public static string ToSendInvitationGroupEmail(this string resource, string fullname, string groupName, string acceptUrl, string declineUrl)
        {
            return resource.Replace("#FULLNAME#", fullname).Replace("#GROUPNAME#", groupName)
                .Replace("#ACCEPTINVIATIONURL#", acceptUrl)
                .Replace("#DECLINEINVIATIONURL#", declineUrl);
        }

        public static int GetUserIdFromToken(this ClaimsPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.UserData)?.Value;
            return int.Parse(userId);
        }

        public static string GetUserRoleFromToken(this ClaimsPrincipal user)
        {
            var claimsIdentity = user.Identity as ClaimsIdentity;
            var role = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value;
            return role;
        }

        public static DateTime FromUTCToLocalTime(this DateTime dt, int timezoneOffset)
        {
            // Convert a DateTime from UTC timezone into the user's timezone.

            return dt.AddHours(timezoneOffset);
        }
    }
}
