using System.ComponentModel;

namespace MismeAPI.Common.DTO
{
    public enum FeedGoupType
    {
        [Description("Proteína animal")]
        ANIMAL_PROTEIN,

        [Description("Proteína vegetal")]
        VEGETABLE_PROTEIN,

        [Description("Lácteos y derivados")]
        LACTEOS,

        [Description("Frutas")]
        FRUITS,

        [Description("Vegetales y verduras")]
        VEGETABLES,

        [Description("Postres/ dulces/ complementos")]
        CANDIES,

        [Description("Grasas/ aderezos/ salsas")]
        FATS,

        [Description("Sopas/ cereales/ tubérculos")]
        SOUPS,

        [Description("Bebidas")]
        DRINKS,

        [Description("Platos combinados")]
        COMPOSE_DISH,
    }

    public class FeedReport
    {
        public FeedGoupType Group { get; set; }
        public string GroupName { get; set; }
        public double Total { get; set; }
        public double Avg { get; set; }
        public double Kcal { get; set; }
        public double AvgKcal { get; set; }
    }
}