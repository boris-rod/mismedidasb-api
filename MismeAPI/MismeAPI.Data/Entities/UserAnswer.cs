using System;

namespace MismeAPI.Data.Entities
{
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