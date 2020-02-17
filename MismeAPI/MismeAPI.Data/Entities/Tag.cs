using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("tag")]
    public class Tag
    {
        public Tag()
        {
            DishTags = new HashSet<DishTag>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<DishTag> DishTags { get; set; }
    }
}