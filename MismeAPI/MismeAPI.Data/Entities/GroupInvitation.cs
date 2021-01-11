using MismeAPI.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("group")]
    public class GroupInvitation
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }
        public string SecurityToken { get; set; }
        public StatusInvitatonEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
