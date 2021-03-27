using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("menueatdish")]
    public class MenuEatDish
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; }
        public int MenuEatId { get; set; }
        public MenuEat MenuEat { get; set; }
        public double Qty { get; set; }

        [NotMapped]
        public double? Calories
        {
            get
            {
                return Dish.Calories * Qty;
            }
        }

        [NotMapped]
        public double? Proteins
        {
            get
            {
                return Dish.Proteins * Qty;
            }
        }

        [NotMapped]
        public double? Carbohydrates
        {
            get
            {
                return Dish.Carbohydrates * Qty;
            }
        }

        [NotMapped]
        public double? Fats
        {
            get
            {
                return Dish.Fat * Qty;
            }
        }

        [NotMapped]
        public double? Fibers
        {
            get
            {
                return Dish.Fiber * Qty;
            }
        }
    }
}
