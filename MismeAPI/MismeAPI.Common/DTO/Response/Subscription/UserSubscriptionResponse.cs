using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.Subscription
{
    public class UserSubscriptionResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int UserSubscriptionId { get; set; }
        public int ProductId { get; set; }
        public string Product { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public TimeSpan ValidDays { get; set; }
        public DateTime ValidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
