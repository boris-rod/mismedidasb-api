using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request
{
    public class CreateDishRequest
    {
        public string Name { get; set; }
        public int Calories { get; set; }
        public double Carbohydrates { get; set; }
        public double Proteins { get; set; }
        public double Fat { get; set; }
        public double Fiber { get; set; }
        public List<TagRequest> Tags { get; set; }
        public IFormFile Image { get; set; }
    }
}