using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("schedule")]
    public class Schedule
    {
        public Schedule()
        {
            EatSchedules = new HashSet<EatSchedule>();
            UserSchedules = new HashSet<UserSchedule>();
            UserSubscriptionSchedules = new HashSet<UserSubscriptionSchedule>();
        }

        public int Id { get; set; }
        public string JobId { get; set; }
        public bool IsProcessed { get; set; }
        public virtual ICollection<EatSchedule> EatSchedules { get; set; }
        public virtual ICollection<UserSchedule> UserSchedules { get; set; }
        public virtual ICollection<UserSubscriptionSchedule> UserSubscriptionSchedules { get; set; }
    }
}
