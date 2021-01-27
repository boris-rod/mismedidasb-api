using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Group
{
    public class GroupInviteActionRequest
    {
        public GroupInviteActionRequest()
        {
            Emails = new HashSet<EmailRequest>();
        }

        public ICollection<EmailRequest> Emails { get; set; }
    }
}
