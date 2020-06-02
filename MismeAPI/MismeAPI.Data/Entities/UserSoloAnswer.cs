using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("usersoloanswer")]
    public class UserSoloAnswer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? SoloAnswerId { get; set; }
        public SoloAnswer SoloAnswer { get; set; }
        public string QuestionCode { get; set; }
        public string AnswerCode { get; set; }
        public string AnswerValue { get; set; }
        public int Points { get; set; }
        public int Coins { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
