using MismeAPI.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("rewardacumulate")]
    public class RewardAcumulate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int RewardCategoryId { get; set; }
        public RewardCategory RewardCategory { get; set; }
        public int Points { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
