using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("rewardcategory")]
    public class RewardCategory
    {
        public RewardCategory()
        {
            RewardAcumulates = new HashSet<RewardAcumulate>();
            RewardHistories = new HashSet<RewardHistory>();
        }

        public int Id { get; set; }
        public RewardCategoryEnum Category { get; set; }
        public string Description { get; set; }

        [DefaultValue(-1)]
        public int MaxPointsAllowed { get; set; }

        public int PointsToIncrement { get; set; }

        [DefaultValue(0)]
        public int PointsToDecrement { get; set; }

        public virtual ICollection<RewardAcumulate> RewardAcumulates { get; set; }
        public virtual ICollection<RewardHistory> RewardHistories { get; set; }
    }
}
