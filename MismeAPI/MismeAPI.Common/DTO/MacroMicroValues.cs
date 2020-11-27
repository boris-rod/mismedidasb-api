using System.ComponentModel;

namespace MismeAPI.Common.DTO
{
    public enum MacroMicroType
    {
        [Description("Proteínas (g)")]
        PROTEINS,

        [Description("Carbohidratos (g)")]
        CARBOHIDRATES,

        [Description("Grasas (g)")]
        FAT,

        [Description("Ac grasos saturados (g)")]
        FAT_ACID_SATURATE,

        [Description("Ac grasos monoinsaturados (g)")]
        FAT_ACID_MONOINSATURATE,

        [Description("Ac grasos polinsaturados (g)")]
        FAT_ACID_POLYINSATURATE,

        [Description("Colesterol (mg)")]
        CHOLESTEROL,

        [Description("Vitamina A (mcg)")]
        VITAMINA,

        [Description("Tiamina (mg)")]
        TIAMIN,

        [Description("Riboflavina (mg)")]
        RIBOFLAVIN,

        [Description("Niacina (mg)")]
        NIACIN,

        [Description("Vitamina B6 (mg)")]
        VITAMIN_B6,

        [Description("Folatos (mcg)")]
        FOLATE,

        [Description("Vitamina B12 (mcg)")]
        VITAMIN_B12,

        [Description("Vitamina C (mg)")]
        VITAMIN_C,

        [Description("Vitamina D (mcg)")]
        VITAMIN_D,

        [Description("Vitamina E (mg)")]
        VITAMIN_E,

        [Description("Vitamina K (mcg)")]
        VITAMIN_K,

        [Description("Calcio (mg)")]
        CALCIUM,

        [Description("Fósforo (mg)")]
        PHOSPHORUS,

        [Description("Hierro (mg)")]
        IRON,

        [Description("Zinc (mg)")]
        ZINC,

        [Description("Potasio (mg)")]
        POTASSIUM,

        [Description("Sodio (mg)")]
        SODIUM,

        [Description("Fibra (g)")]
        FIBER,

        [Description("Alcohol (g)")]
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