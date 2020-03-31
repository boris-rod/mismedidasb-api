using MismeAPI.Common.DTO.Request.Answer;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request.Question
{
    public class AddOrUpdateQuestionWithAnswersRequest
    {
        public int PollId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionName { get; set; }
        public int QuestionOrder { get; set; }
        public List<BasicAnswerRequest> Answers { get; set; }
    }
}