using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("eatcompounddish")]
    public class EatCompoundDish
    {
        public int Id { get; set; }
        public int CompoundDishId { get; set; }
        public CompoundDish CompoundDish { get; set; }
        public int EatId { get; set; }
        public Eat Eat { get; set; }
        public int Qty { get; set; }
    }
}