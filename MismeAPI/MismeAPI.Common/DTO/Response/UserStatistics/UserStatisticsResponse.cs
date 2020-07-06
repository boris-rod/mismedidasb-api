namespace MismeAPI.Common.DTO.Response.UserStatistics
{
    public class UserStatisticsResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Points { get; set; }
        public int Coins { get; set; }
        public int EatCurrentStreak { get; set; }
        public int EatMaxStreak { get; set; }
        public int BalancedEatCurrentStreak { get; set; }
        public int BalancedEatMaxStreak { get; set; }
        public int TotalNonBalancedEatsPlanned { get; set; }
        public int TotalBalancedEatsPlanned { get; set; }
        public UserRankingResponse PersonalRanking { get; set; }
        public UserResponse User { get; set; }
    }
}
