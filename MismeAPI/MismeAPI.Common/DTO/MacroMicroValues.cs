using System.ComponentModel;

namespace MismeAPI.Common.DTO
{
    public enum MacroMicroType
    {
        [Description("Proteínas")]
        PROTEINS,

        [Description("Carbohidratos")]
        CARBOHIDRATES,

        [Description("Grasas")]
        FAT,

        [Description("Ac grasos saturados")]
        FAT_ACID_SATURATE,

        [Description("Ac grasos monoinsaturados")]
        FAT_ACID_MONOINSATURATE,

        [Description("Ac grasos polinsaturados")]
        FAT_ACID_POLYINSATURATE,

        [Description("Colesterol")]
        CHOLESTEROL,

        [Description("Vitamina A")]
        VITAMINA,

        [Description("Tiamina")]
        TIAMIN,

        [Description("Riboflavina")]
        RIBOFLAVIN,

        [Description("Niacina")]
        NIACIN,

        [Description("Vitamina B6")]
        VITAMIN_B6,

        [Description("Folatos")]
        FOLATE,

        [Description("Vitamina B12")]
        VITAMIN_B12,

        [Description("Vitamina C")]
        VITAMIN_C,

        [Description("Vitamina D")]
        VITAMIN_D,

        [Description("Vitamina E")]
        VITAMIN_E,

        [Description("Vitamina K")]
        VITAMIN_K,

        [Description("Calcio")]
        CALCIUM,

        [Description("Fósforo")]
        PHOSPHORUS,

        [Description("Hierro")]
        IRON,

        [Description("Zinc")]
        ZINC,

        [Description("Potasio")]
        POTASSIUM,

        [Description("Sodio")]
        SODIUM,

        [Description("Fibra")]
        FIBER,

        [Description("Alcohol")]
        ALCOHOL
    }

    public class MacroMicroValues
    {
        public double Total { get; set; }
        public double Avg { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public MacroMicroType Type { get; set; }
        public string TypeString { get; set; }
        public string Name { get; set; }
    }
}