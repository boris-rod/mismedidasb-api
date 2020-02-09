using System;
using System.Collections.Generic;

namespace MismeAPI.Data.Entities
{
    public class Question
    {
        public Question()
        {
            Answers = new HashSet<Answer>();
        }

        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<Answer> Answers { get; set; }
    }
}