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
    }
}
