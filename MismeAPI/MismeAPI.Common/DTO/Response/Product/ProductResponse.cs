using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.Product
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public string NameIT { get; set; }
        public string Description { get; set; }
        public string DescriptionEN { get; set; }
        public string DescriptionIT { get; set; }
        public decimal Price { get; set; }
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
