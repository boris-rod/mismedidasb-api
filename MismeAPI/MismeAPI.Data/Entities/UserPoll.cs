using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("userpoll")]
    public class UserPoll
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
        public string Result { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}