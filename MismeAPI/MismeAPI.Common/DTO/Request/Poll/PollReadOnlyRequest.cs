using MismeAPI.Common.DTO.Request.Question;

namespace MismeAPI.Common.DTO.Request.Poll
{
    public class PollReadOnlyRequest
    {
        public bool ReadOnly { get; set; }
        public string HtmlContent { get; set; }
        public AddOrUpdateQuestionWithAnswersRequest QuestionWithAnswers { get; set; }
    }
}