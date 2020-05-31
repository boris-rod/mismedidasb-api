using MismeAPI.Common.DTO.Response.CompoundDish;

namespace MismeAPI.Common.DTO.Response
{
    public class EatCompoundDishResponse
    {
        public CompoundDishResponse CompoundDish { get; set; }
        public int Qty { get; set; }
    }
}