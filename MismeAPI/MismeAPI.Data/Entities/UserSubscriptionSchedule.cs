using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("usersubscriptionschedule")]
    public class UserSubscriptionSchedule
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public int UserSubscriptionId { get; set; }
        public UserSubscription UserSubscription { get; set; }
    }
}
