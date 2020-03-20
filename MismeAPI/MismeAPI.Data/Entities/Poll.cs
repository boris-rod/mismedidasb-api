﻿using System;
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
        }

        public int Id { get; set; }
        public int ConceptId { get; set; }
        public Concept Concept { get; set; }
        public string Name { get; set; }
        public string Codename { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public int Order { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
    }
}