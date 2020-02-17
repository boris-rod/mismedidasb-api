using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("useranswer")]
    public class UserAnswer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int AnswerId { get; set; }
        public Answer Answer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}