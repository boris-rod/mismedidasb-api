using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response
{
    public class DishResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public double Carbohydrates { get; set; }
        public double Proteins { get; set; }
        public double Fat { get; set; }
        public double Fiber { get; set; }
        public List<TagResponse> Tags { get; set; }
    }
}