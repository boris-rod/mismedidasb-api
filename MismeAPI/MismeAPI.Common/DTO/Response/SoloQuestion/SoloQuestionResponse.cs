using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response.SoloQuestion
{
    public class SoloQuestionResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string TitleEN { get; set; }
        public string TitleIT { get; set; }
        public bool AllowCustomAnswer { get; set; }
        public virtual ICollection<SoloAnswerResponse> SoloAnswers { get; set; }
    }
}
