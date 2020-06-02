using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.SoloAnswer
{
    public class CreateSoloAnswerRequest
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public string TitleEN { get; set; }
        public string TitleIT { get; set; }
        public int Points { get; set; }
    }
}
