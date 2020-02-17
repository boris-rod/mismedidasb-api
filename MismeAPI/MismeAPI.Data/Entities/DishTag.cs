using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("dishtag")]
    public class DishTag
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
        public DateTime TaggedAt { get; set; }
    }
}