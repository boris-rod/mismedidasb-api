using MismeAPI.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("userschedule")]
    public class UserSchedule
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string JobConstant { get; set; }
        public string UserTimeZone { get; set; }
    }
}
