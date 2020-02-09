using System;
using System.Collections.Generic;

namespace MismeAPI.Data.Entities
{
    public class Concept
    {
        public Concept()
        {
            Polls = new HashSet<Poll>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Codename { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<Poll> Polls { get; set; }
    }
}