using MismeAPI.Common.DTO.Request;
using System;

namespace MismeAPI.Common.DTO.Menu
{
    public class CreateMenuEatRequest
    {
        public int EatType { get; set; }

        public BasicDishQtyRequest[] Dishes { get; set; }
        public BasicDishQtyRequest[] CompoundDishes { get; set; }
    }
}
