using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MismeAPI.Data.Entities
{
    [Table("userreferral")]
    public class UserReferral
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int? InvitedId { get; set; }
        public User Invited { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
