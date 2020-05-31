namespace MismeAPI.Common.DTO.Request
{
    public class CreateEatRequest
    {
        public int EatType { get; set; }
        public BasicDishQtyRequest[] Dishes { get; set; }
        public BasicDishQtyRequest[] CompoundDishes { get; set; }
    }
}