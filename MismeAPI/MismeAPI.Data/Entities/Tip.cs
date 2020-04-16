using MismeAPI.Data.Entities.Enums;
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
        public TipPositionEnum TipPosition { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ContentEN { get; set; }
        public string ContentIT { get; set; }
    }
}