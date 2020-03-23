using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("tip")]
    public class Tip
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public Poll Poll { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}