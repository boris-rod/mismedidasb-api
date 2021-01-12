using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MismeAPI.Common.DTO.Response.Order
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public string ExternalId { get; set; }
        public int? UserId { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal Amount { get; set; }

        public int StatusId { get; set; }
        public string Status { get; set; }
        public string StatusInformation { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
