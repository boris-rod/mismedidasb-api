using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("eatschedule")]
    public class EatSchedule
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public int EatId { get; set; }
        public Eat Eat { get; set; }
    }
}
