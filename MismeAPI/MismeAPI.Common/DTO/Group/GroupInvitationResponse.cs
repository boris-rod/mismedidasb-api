using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Group
{
    public class GroupInvitationResponse
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
