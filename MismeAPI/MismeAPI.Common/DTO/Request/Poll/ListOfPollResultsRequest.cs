using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request.Poll
{
    public class ListOfPollResultsRequest
    {
        public List<SetPollResultWithQuestionsRequest> PollDatas { get; set; }
    }
}