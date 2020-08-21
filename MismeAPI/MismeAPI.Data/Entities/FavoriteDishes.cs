using MismeAPI.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("favoritedish")]
    public class FavoriteDish
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public Dish Dish { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
