using MismeAPI.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("orders")]
    public class Order
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public int? ProductId { get; set; }
        public Product Product { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        public OrderStatusEnum Status { get; set; }
        public string StatusInformation { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string ObjectType { get; set; }
    }
}
