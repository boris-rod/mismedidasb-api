using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.Reward
{
    public class CreateRewardRequest
    {
        public int UserId { get; set; }
        public int RewardCategoryEnum { get; set; }
        public bool IsPlus;
        public string Data { get; set; }
    }
}
