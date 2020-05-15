using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("userstatics")]
    public class UserStatistics
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int Points { get; set; }
        public int EatCurrentStreak { get; set; }
        public int EatMaxStreak { get; set; }
        public int BalancedEatCurrentStreak { get; set; }
        public int BalancedEatMaxStreak { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
