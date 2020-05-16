using System;
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
            DishCompoundDishes = new HashSet<DishCompoundDish>();
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
        public double Calcium { get; set; }
        public double Phosphorus { get; set; }
        public double Iron { get; set; }
        public double Potassium { get; set; }
        public double Sodium { get; set; }
        public double Zinc { get; set; }
        public double Magnesium { get; set; }
        public double Thiamine { get; set; }
        public double Ribofla { get; set; }
        public double Niacin { get; set; }
        public double FolicAcid { get; set; }
        public double VitaminA { get; set; }
        public double VitaminC { get; set; }
        public double VitaminB6 { get; set; }
        public double VitaminB12 { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<DishTag> DishTags { get; set; }
        public virtual ICollection<EatDish> EatDishes { get; set; }
        public virtual ICollection<DishCompoundDish> DishCompoundDishes { get; set; }
    }
}