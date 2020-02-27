using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("eatdish")]
    public class EatDish
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; }
        public int EatId { get; set; }
        public Eat Eat { get; set; }
        public int Qty { get; set; }
    }
}