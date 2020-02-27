using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("eat")]
    public class Eat
    {
        public Eat()
        {
            EatDishes = new HashSet<EatDish>();
        }

        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public EatTypeEnum EatType { get; set; }
        public virtual ICollection<EatDish> EatDishes { get; set; }
    }
}