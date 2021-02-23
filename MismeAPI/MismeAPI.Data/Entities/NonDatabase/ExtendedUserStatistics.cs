using MismeAPI.Common.DTO.Response.SoloQuestion;
using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Data.Entities.NonDatabase
{
    public class ExtendedUserStatistics
    {
        public ExtendedUserStatistics()
        {
            MostFrequentEmotions = new List<int>();
            LastEmotionsReported = new List<UserSoloAnswerResponse>();
        }

        public int BestComplyEatStreak { get; set; }
        public int TotalDaysPlannedSport { get; set; }
        public int TotalDaysComplySportPlan { get; set; }
        public ICollection<int> MostFrequentEmotions { get; set; }
        public int MostFrequentEmotionCount { get; set; }
        public double? EmotionMedia { get; set; }

        /// <summary>
        /// Last answers of the user saying his emotions, how I felt today
        /// </summary>
        public ICollection<UserSoloAnswerResponse> LastEmotionsReported { get; set; }

        /// <summary>
        /// List of answers where the user said that he gonna do excersices.
        /// </summary>
        public ICollection<UserSoloAnswerResponse> WillDoSportReportedDays { get; set; }

        /// <summary>
        /// List of answers where the user said if he did the planned excersices or not
        /// </summary>
        public ICollection<UserSoloAnswerResponse> AccomplishSportPlannedReportedDays { get; set; }
    }
}
