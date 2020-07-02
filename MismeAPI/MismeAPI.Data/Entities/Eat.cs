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
            EatCompoundDishes = new HashSet<EatCompoundDish>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public EatTypeEnum EatType { get; set; }
        public bool IsBalanced { get; set; }

        /// <summary>
        /// UTC datetime when the user plan to take this eat
        /// </summary>
        public DateTime? EatUtcAt { get; set; }

        /// <summary>
        /// Datetime sent by user to register when he created this eat. Do not update after current
        /// day 9:00 AM local user time
        /// </summary>
        public DateTime? PlanCreatedAt { get; set; }

        /// <summary>
        /// Mark if this was a Balanced plan before become a reported eat. Do not update after
        /// current day 9:00 AM local user time
        /// </summary>
        public bool? IsBalancedPlan { get; set; }

        public EatSchedule EatSchedule { get; set; }

        public virtual ICollection<EatDish> EatDishes { get; set; }
        public virtual ICollection<EatCompoundDish> EatCompoundDishes { get; set; }

        public double KCalAtThatMoment { get; set; }
        public double ImcAtThatMoment { get; set; }
    }
}