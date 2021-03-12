﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MismeAPI.Data.Entities
{
    [Table("menu")]
    public class Menu
    {
        public Menu()
        {
            Eats = new HashSet<MenuEat>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public string NameIT { get; set; }
        public string Description { get; set; }
        public string DescriptionEN { get; set; }
        public string DescriptionIT { get; set; }
        public bool Active { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }
        public int? CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<MenuEat> Eats { get; set; }
    }
}
