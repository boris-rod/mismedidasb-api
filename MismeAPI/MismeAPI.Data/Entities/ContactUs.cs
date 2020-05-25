using MismeAPI.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("contactus")]
    public class ContactUs
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Read { get; set; }
        public ContactPriorityEnum Priority { get; set; }
        public bool IsAnswered { get; set; }
    }
}