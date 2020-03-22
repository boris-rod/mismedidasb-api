using MismeAPI.Common.DTO.Request.Question;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request.Poll
{
    public class SetPollResultWithQuestionsRequest
    {
        public List<QuestionResultRequest> QuestionsResults { get; set; }
    }
}