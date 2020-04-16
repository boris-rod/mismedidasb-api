using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("concept")]
    public class Concept
    {
        public Concept()
        {
            Polls = new HashSet<Poll>();
            UserConcepts = new HashSet<UserConcept>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Codename { get; set; }
        public string Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<Poll> Polls { get; set; }
        public virtual ICollection<UserConcept> UserConcepts { get; set; }

        public string TitleEN { get; set; }
        public string DescriptionEN { get; set; }
        public string TitleIT { get; set; }
        public string DescriptionIT { get; set; }
    }
}