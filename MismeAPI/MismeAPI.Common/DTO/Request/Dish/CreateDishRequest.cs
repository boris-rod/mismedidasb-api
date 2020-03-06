using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request
{
    public class CreateDishRequest
    {
        public CreateDishRequest()
        {
            NewTags = new List<string>();
            TagsIds = new List<int>();
        }

        public string Name { get; set; }
        public double Calories { get; set; }
        public double Carbohydrates { get; set; }
        public double Proteins { get; set; }
        public double Fat { get; set; }
        public double Fiber { get; set; }
        public List<int> TagsIds { get; set; }
        public List<string> NewTags { get; set; }
        public IFormFile Image { get; set; }
    }
}