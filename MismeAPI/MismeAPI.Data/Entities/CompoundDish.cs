using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("compounddish")]
    public class CompoundDish
    {
        public CompoundDish()
        {
            DishCompoundDishes = new HashSet<DishCompoundDish>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public User CreatedBy { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string ImageMimeType { get; set; }

        public virtual ICollection<DishCompoundDish> DishCompoundDishes { get; set; }
    }
}