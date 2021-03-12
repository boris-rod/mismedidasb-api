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
    }
}
