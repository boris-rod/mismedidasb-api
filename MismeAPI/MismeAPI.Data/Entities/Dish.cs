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
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public double Carbohydrates { get; set; }
        public double Proteins { get; set; }
        public double Fat { get; set; }
        public double Fiber { get; set; }
        public string Image { get; set; }
        public string ImageMimeType { get; set; }
        public virtual ICollection<DishTag> DishTags { get; set; }
    }
}