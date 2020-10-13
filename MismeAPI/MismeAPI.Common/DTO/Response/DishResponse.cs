using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response
{
    public class DishResponse
    {
        public DishResponse()
        {
            Tags = new HashSet<TagResponse>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsLackSelfControlDish { get; set; }
        public int LackSelfControlDishIntensity { get; set; }
        public double Calories { get; set; }
        public double Carbohydrates { get; set; }
        public double Proteins { get; set; }
        public double Fat { get; set; }
        public double Fiber { get; set; }
        public ICollection<TagResponse> Tags { get; set; }
        public string Image { get; set; }
        public string ImageMimeType { get; set; }

        public bool IsProteic { get; set; }
        public bool IsCaloric { get; set; }
        public bool IsFruitAndVegetables { get; set; }
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
        public int HandCode { get; set; }
    }
}