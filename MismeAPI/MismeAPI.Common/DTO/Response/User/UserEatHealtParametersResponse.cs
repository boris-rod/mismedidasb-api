using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.User
{
    public class UserEatHealtParametersResponse
    {
        public double KCalOffSetVal { get; set; }
        public double KCalMin { get; set; }
        public double KCalMax { get; set; }
        public double BreakFastCalVal { get; set; }
        public double BreakFastCalValExtra { get; set; }
        public double Snack1CalVal { get; set; }
        public double Snack1CalValExtra { get; set; }
        public double LunchCalVal { get; set; }
        public double LunchCalValExtra { get; set; }
        public double Snack2CalVal { get; set; }
        public double Snack2CalValExtra { get; set; }
        public double DinnerCalVal { get; set; }
        public double DinnerCalValExtra { get; set; }
    }
}
