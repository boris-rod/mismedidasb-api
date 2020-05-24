using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response
{
    public class UserReferralResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? InvitedId { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
