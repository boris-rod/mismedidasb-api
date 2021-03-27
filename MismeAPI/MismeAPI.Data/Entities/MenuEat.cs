using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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

        [NotMapped]
        public double? Calories
        {
            get
            {
                return EatDishes.Sum(ed => ed.Calories) + EatCompoundDishes.Sum(ecd => ecd.Calories);
            }
        }

        [NotMapped]
        public double? Proteins
        {
            get
            {
                return EatDishes.Sum(ed => ed.Proteins) + EatCompoundDishes.Sum(ecd => ecd.Proteins);
            }
        }

        [NotMapped]
        public double? Carbohydrates
        {
            get
            {
                return EatDishes.Sum(ed => ed.Carbohydrates) + EatCompoundDishes.Sum(ecd => ecd.Carbohydrates);
            }
        }

        [NotMapped]
        public double? Fats
        {
            get
            {
                return EatDishes.Sum(ed => ed.Fats) + EatCompoundDishes.Sum(ecd => ecd.Fats);
            }
        }

        [NotMapped]
        public double? Fibers
        {
            get
            {
                return EatDishes.Sum(ed => ed.Fibers) + EatCompoundDishes.Sum(ecd => ecd.Fibers);
            }
        }

        public virtual ICollection<MenuEatDish> EatDishes { get; set; }
        public virtual ICollection<MenuEatCompoundDish> EatCompoundDishes { get; set; }
    }
}
