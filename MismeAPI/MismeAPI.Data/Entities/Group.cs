﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("group")]
    public class Group
    {
        public Group()
        {
            Invitations = new HashSet<GroupInvitation>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public string AdminEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<GroupInvitation> Invitations { get; set; }
    }
}
