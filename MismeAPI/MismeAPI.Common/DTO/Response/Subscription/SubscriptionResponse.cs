using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.Subscription
{
    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Product { get; set; }
        public int ValueCoins { get; set; }
        public bool IsActive { get; set; }
        public TimeSpan ValidDays { get; set; }
        public DateTime ValidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
