using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response
{
    public class UserRankingResponse
    {
        public int Points { get; set; }
        public int RankingPosition { get; set; }
        public int PercentageBehind { get; set; }
    }
}
