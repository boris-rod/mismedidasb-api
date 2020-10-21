namespace MismeAPI.Common.DTO
{
    public enum MacroMicroType
    {
        PROTEINS,
        CARBOHIDRATES,
        FAT,
        FAT_ACID_SATURATE,
        FAT_ACID_MONOINSATURATE,
        FAT_ACID_POLYINSATURATE,
        CHOLESTEROL,
        VITAMINA,
        TIAMIN,
        RIBOFLAVIN,
        NIACIN,
        VITAMIN_B6,
        FOLATE,
        VITAMIN_B12,
        VITAMIN_C,
        VITAMIN_D,
        VITAMIN_E,
        VITAMIN_K,
        CALCIUM,
        PHOSPHORUS,
        IRON,
        ZINC,
        POTASSIUM,
        SODIUM,
        FIBER,
        ALCOHOL
    }

    public class MacroMicroValues
    {
        public double Total { get; set; }
        public double Avg { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public MacroMicroType Type { get; set; }
        public string Name { get; set; }
    }
}