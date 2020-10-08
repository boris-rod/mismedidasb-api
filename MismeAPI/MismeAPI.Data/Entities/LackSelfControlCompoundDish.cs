using MismeAPI.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("lackselfcontrolcompounddishes")]
    public class LackSelfControlCompoundDish
    {
        public int Id { get; set; }
        public int DishId { get; set; }
        public CompoundDish Dish { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int Intensity { get; set; }
    }
}
