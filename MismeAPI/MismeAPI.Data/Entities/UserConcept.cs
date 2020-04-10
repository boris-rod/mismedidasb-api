using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("userconcept")]
    public class UserConcept
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ConceptId { get; set; }
        public Concept Concept { get; set; }
        public string Result { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}