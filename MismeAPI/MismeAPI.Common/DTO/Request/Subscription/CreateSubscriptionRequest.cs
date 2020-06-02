using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.Subscription
{
    public class CreateSubscriptionRequest
    {
        public int Product { get; set; }
        public string Name { get; set; }
        public int ValueCoins { get; set; }
        public bool IsActive { get; set; }
    }
}
