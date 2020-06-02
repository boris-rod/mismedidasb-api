using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("subscription")]
    public class Subscription
    {
        public Subscription()
        {
            Subscribers = new HashSet<UserSubscription>();
        }

        public int Id { get; set; }
        public SubscriptionEnum Product { get; set; }
        public string Name { get; set; }
        public int ValueCoins { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public virtual ICollection<UserSubscription> Subscribers { get; set; }
    }
}
