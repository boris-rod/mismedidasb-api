using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MismeAPI.Data.Entities
{
    [Table("groupserviceprice")]
    public class GroupServicePrice
    {
        public int Id { get; set; }
        public int ServicePriceId { get; set; }
        public ServicePrice ServicePrice { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public bool IsValid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime ValidAt { get; set; }
    }
}
