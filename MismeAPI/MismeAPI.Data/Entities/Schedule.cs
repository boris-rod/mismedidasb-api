using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("schedule")]
    public class Schedule
    {
        public int Id { get; set; }
        public string JobId { get; set; }
        public bool IsProcessed { get; set; }
    }
}
