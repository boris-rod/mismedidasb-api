namespace MismeAPI.Common.DTO.Request.Question
{
    public class QuestionResultRequest
    {
        public int PollId { get; set; }
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }
}