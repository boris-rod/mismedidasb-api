using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.SoloQuestion
{
    public class UserSoloAnswerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? SoloAnswerId { get; set; }
        public string QuestionCode { get; set; }
        public string AnswerCode { get; set; }
        public string AnswerValue { get; set; }
        public int Points { get; set; }
        public int Coins { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
