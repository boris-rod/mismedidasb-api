using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("dish")]
    public class Dish
    {
        public Dish()
        {
            DishTags = new HashSet<DishTag>();
            EatDishes = new HashSet<EatDish>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public string NameIT { get; set; }
        public double Calories { get; set; }
        public double Carbohydrates { get; set; }
        public double Proteins { get; set; }
        public double Fat { get; set; }
        public double Fiber { get; set; }
        public string Image { get; set; }
        public string ImageMimeType { get; set; }
        public bool IsProteic { get; set; }
        public bool IsCaloric { get; set; }
        public bool IsFruitAndVegetables { get; set; }
        public double Cholesterol { get; set; }
        public double Vitamins { get; set; }
        public virtual ICollection<DishTag> DishTags { get; set; }
        public virtual ICollection<EatDish> EatDishes { get; set; }
    }
}