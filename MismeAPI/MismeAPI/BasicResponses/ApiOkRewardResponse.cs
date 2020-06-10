using MismeAPI.Common.DTO.Response.Reward;

namespace MismeAPI.BasicResponses
{
    public class ApiOkRewardResponse : ApiResponse

    {
        public ApiOkRewardResponse(object result, RewardResponse reward)
            : base(200)
        {
            Result = result;
            Reward = reward;
        }

        public object Result { get; }
        public RewardResponse Reward { get; }
    }
}
