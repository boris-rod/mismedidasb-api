using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("menueatcompounddish")]
    public class MenuEatCompoundDish
    {
        public int Id { get; set; }
        public int CompoundDishId { get; set; }
        public CompoundDish CompoundDish { get; set; }
        public int MenuEatId { get; set; }
        public MenuEat MenuEat { get; set; }
        public double Qty { get; set; }

        [NotMapped]
        public double? Calories
        {
            get
            {
                double? calories = 0;
                foreach (var eatCompoundDish in CompoundDish.EatCompoundDishes)
                {
                    foreach (var dishCompoundDish in eatCompoundDish.CompoundDish.DishCompoundDishes)
                    {
                        var dishCals = dishCompoundDish.Dish.Calories.HasValue ? dishCompoundDish.Dish.Calories.Value : 0;
                        calories += dishCals * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                    }
                }
                return calories;
            }
        }

        [NotMapped]
        public double? Proteins
        {
            get
            {
                double? proteins = 0;
                foreach (var eatCompoundDish in CompoundDish.EatCompoundDishes)
                {
                    foreach (var dishCompoundDish in eatCompoundDish.CompoundDish.DishCompoundDishes)
                    {
                        var prot = dishCompoundDish.Dish.Proteins.HasValue ? dishCompoundDish.Dish.Proteins.Value : 0;
                        proteins += prot * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                    }
                }
                return proteins;
            }
        }

        [NotMapped]
        public double? Carbohydrates
        {
            get
            {
                double? carbohydrates = 0;
                foreach (var eatCompoundDish in CompoundDish.EatCompoundDishes)
                {
                    foreach (var dishCompoundDish in eatCompoundDish.CompoundDish.DishCompoundDishes)
                    {
                        var carb = dishCompoundDish.Dish.Carbohydrates.HasValue ? dishCompoundDish.Dish.Carbohydrates.Value : 0;
                        carbohydrates += carb * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                    }
                }
                return carbohydrates;
            }
        }

        [NotMapped]
        public double? Fats
        {
            get
            {
                double? fats = 0;
                foreach (var eatCompoundDish in CompoundDish.EatCompoundDishes)
                {
                    foreach (var dishCompoundDish in eatCompoundDish.CompoundDish.DishCompoundDishes)
                    {
                        var fat = dishCompoundDish.Dish.Fat.HasValue ? dishCompoundDish.Dish.Fat.Value : 0;
                        fats += fat * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                    }
                }
                return fats;
            }
        }

        [NotMapped]
        public double? Fibers
        {
            get
            {
                double? fibers = 0;
                foreach (var eatCompoundDish in CompoundDish.EatCompoundDishes)
                {
                    foreach (var dishCompoundDish in eatCompoundDish.CompoundDish.DishCompoundDishes)
                    {
                        var fib = dishCompoundDish.Dish.Fiber.HasValue ? dishCompoundDish.Dish.Fiber.Value : 0;
                        fibers += fib * eatCompoundDish.Qty * dishCompoundDish.DishQty;
                    }
                }
                return fibers;
            }
        }
    }
}
