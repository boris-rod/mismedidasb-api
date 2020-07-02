using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Data.Entities.NonDatabase
{
    public class ExtendedUserStatistics
    {
        public int BestComplyEatStreak { get; set; }
        public int TotalDaysPlannedSport { get; set; }
        public int TotalDaysComplySportPlan { get; set; }
        public int MostFrequentEmotion { get; set; }
        public int MostFrequentEmotionCount { get; set; }
    }
}
