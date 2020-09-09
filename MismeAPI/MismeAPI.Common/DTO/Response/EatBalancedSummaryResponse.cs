using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response
{
    public class EatBalancedSummaryResponse
    {
        public double CurrentCaloriesSum { get; set; }
        public double CurrentProteinsSum { get; set; }
        public double CurrentCarbohydratesSum { get; set; }
        public double CurrentFatSum { get; set; }
        public double CurrentFiberSum { get; set; }

        public bool IsBreakfastKcalOk { get; set; }
        public bool IsSnack1KcalOk { get; set; }
        public bool IsLunchKcalOk { get; set; }
        public bool IsSnack2KcalOk { get; set; }
        public bool IsDinnerKcalOk { get; set; }

        public double BreakfastKcalPer { get; set; }
        public double Snack1KcalPer { get; set; }
        public double LunchKcalPer { get; set; }
        public double Snack2KcalPer { get; set; }
        public double DinnerKcalPer { get; set; }

        public bool IsKcalAllOk { get; set; }
        public bool IsProteinsOk { get; set; }
        public bool IsCarbohydratesOk { get; set; }
        public bool IsFatOk { get; set; }
        public bool IsFiberOk { get; set; }

        public double ProteinPer { get; set; }
        public double CarbohydratesPer { get; set; }
        public double FatPer { get; set; }
        public double FiberPer { get; set; }

        public bool IsBalanced { get; set; }
    }
}
