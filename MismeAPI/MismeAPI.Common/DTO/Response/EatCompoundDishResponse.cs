using MismeAPI.Common.DTO.Response.CompoundDish;

namespace MismeAPI.Common.DTO.Response
{
    public class EatCompoundDishResponse
    {
        public CompoundDishResponse CompoundDish { get; set; }
        public double Qty { get; set; }
    }
}