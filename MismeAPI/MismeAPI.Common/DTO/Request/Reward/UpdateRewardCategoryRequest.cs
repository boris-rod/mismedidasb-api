using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.Reward
{
    public class UpdateRewardCategoryRequest
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public int MaxPointsAllowed { get; set; }

        public int PointsToIncrement { get; set; }

        public int PointsToDecrement { get; set; }
    }
}
