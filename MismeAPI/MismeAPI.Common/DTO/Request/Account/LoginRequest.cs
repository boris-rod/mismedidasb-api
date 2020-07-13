namespace MismeAPI.Common.DTO.Request
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserTimeZone { get; set; }
        public int UserTimeZoneOffset { get; set; }
    }
}
