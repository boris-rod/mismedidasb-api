using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MismeAPI.Data.Entities
{
    [Table("serviceprice")]
    public class ServicePrice
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public string Name { get; set; }
        public string StripeId { get; set; }
        public int Price { get; set; }
        public string Currency { get; set; }
        public string Interval { get; set; }
    }
}
