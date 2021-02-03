namespace MismeAPI.Common.DTO.Group
{
    public class GroupInviteActionResponse
    {
        public string Email { get; set; }

        public bool SuccessInvited { get; set; }
        public string CustomError { get; set; }
    }
}
