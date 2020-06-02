using MismeAPI.Common.DTO.Request.SoloAnswer;
using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.SoloQuestion
{
    public class CreateSoloQuestionRequest
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string TitleEN { get; set; }
        public string TitleIT { get; set; }
        public bool AllowCustomAnswer { get; set; }
        public virtual ICollection<CreateSoloAnswerRequest> SoloAnswers { get; set; }
    }
}
