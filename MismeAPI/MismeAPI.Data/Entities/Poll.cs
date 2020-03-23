using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("poll")]
    public class Poll
    {
        public Poll()
        {
            Questions = new HashSet<Question>();
            UserPolls = new HashSet<UserPoll>();
            Tips = new HashSet<Tip>();
        }

        public int Id { get; set; }
        public int ConceptId { get; set; }
        public Concept Concept { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int Order { get; set; }
        public bool IsReadOnly { get; set; }
        public string HtmlContent { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<UserPoll> UserPolls { get; set; }
        public virtual ICollection<Tip> Tips { get; set; }
    }
}