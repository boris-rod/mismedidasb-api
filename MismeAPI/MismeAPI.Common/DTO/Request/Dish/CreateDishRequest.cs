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

        public int Classification { get; set; }
        public double Cholesterol { get; set; }
        public double Calcium { get; set; }
        public double Phosphorus { get; set; }
        public double Iron { get; set; }
        public double Potassium { get; set; }
        public double Sodium { get; set; }
        public double Zinc { get; set; }
        public double VitaminA { get; set; }
        public double VitaminC { get; set; }
        public double VitaminB6 { get; set; }
        public double VitaminB12 { get; set; }

        public double VitaminD { get; set; }
        public double VitaminE { get; set; }
        public double VitaminK { get; set; }

        public double VitaminB1Thiamin { get; set; }
        public double VitaminB2Riboflavin { get; set; }
        public double VitaminB3Niacin { get; set; }
        public double VitaminB9Folate { get; set; }
        public double NetWeight { get; set; }
        public double Volume { get; set; }
        public double SaturatedFat { get; set; }
        public double MonoUnsaturatedFat { get; set; }
        public double PolyUnsaturatedFat { get; set; }
        public double Alcohol { get; set; }
    }
}