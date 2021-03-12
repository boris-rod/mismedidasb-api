using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("menueat")]
    public class MenuEat
    {
        public MenuEat()
        {
            EatDishes = new HashSet<MenuEatDish>();
            EatCompoundDishes = new HashSet<MenuEatCompoundDish>();
        }

        public int Id { get; set; }
        public int MenuId { get; set; }
        public Menu Menu { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public EatTypeEnum EatType { get; set; }
        public bool IsBalanced { get; set; }

        public virtual ICollection<MenuEatDish> EatDishes { get; set; }
        public virtual ICollection<MenuEatCompoundDish> EatCompoundDishes { get; set; }
    }
}
