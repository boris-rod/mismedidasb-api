using MismeAPI.Common.DTO.Response;
using MismeAPI.Common.DTO.Response.Reward;
using System.Collections.Generic;

namespace MismeAPI.BasicResponses
{
    public class ApiPlanResponse : ApiResponse

    {
        public ApiPlanResponse(object result, List<PlanSummaryResponse> planSummaries)
            : base(200)
        {
            Result = result;
            PlanSummaries = planSummaries;
        }

        public object Result { get; }
        public List<PlanSummaryResponse> PlanSummaries { get; }
    }
}
