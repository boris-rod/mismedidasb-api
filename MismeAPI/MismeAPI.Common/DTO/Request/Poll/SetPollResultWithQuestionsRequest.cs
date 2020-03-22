using MismeAPI.Common.DTO.Request.Question;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request.Poll
{
    public class SetPollResultWithQuestionsRequest
    {
        public int PollId { get; set; }
        public List<QuestionResultRequest> QuestionsResults { get; set; }
    }
}