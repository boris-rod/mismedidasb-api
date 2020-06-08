using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.Reward
{
    public class RewardResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RewardCategoryId { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }

        public int Points { get; set; }
        public int RewardPoints { get; set; }
        public bool IsPlus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
