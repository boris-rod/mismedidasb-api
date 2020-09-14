using MismeAPI.Common.DTO.Response.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response
{
    public class PlanSummaryResponse
    {
        public DateTime PlanDateTime { get; set; }
        public UserEatHealtParametersResponse UserEatHealtParameters { get; set; }
        public EatBalancedSummaryResponse EatBalancedSummary { get; set; }
    }
}
