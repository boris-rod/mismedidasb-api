using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("dishcompounddish")]
    public class DishCompoundDish
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; }
        public int CompoundDishId { get; set; }
        public CompoundDish CompoundDish { get; set; }
        public int DishQty { get; set; }
    }
}