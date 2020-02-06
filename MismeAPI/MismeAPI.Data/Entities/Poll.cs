using System;
using System.Collections.Generic;

namespace MismeAPI.Data.Entities
{
    public class Poll
    {
        public Poll()
        {
            Questions = new HashSet<Question>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}