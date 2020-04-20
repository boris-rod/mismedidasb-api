namespace MismeAPI.Common.DTO.Response
{
    public class QuestionAdminResponse : QuestionResponse
    {
        public string PollName { get; set; }
        public string TitleEN { get; set; }
        public string TitleIT { get; set; }
    }
}