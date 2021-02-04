using MismeAPI.Common.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MismeAPI.Common.DTO.Group
{
    public class GroupExtendedResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AdminEmail { get; set; }
        public bool IsActive { get; set; }
        public int UsersCount { get => Users.Count(); }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public ICollection<GroupInvitationResponse> Invitations { get; set; }
        public ICollection<UserResponse> Users { get; set; }
    }
}
