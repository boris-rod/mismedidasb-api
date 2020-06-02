using System;

namespace MismeAPI.Common.DTO.Request
{
    public class CreateEatRequest
    {
        public int EatType { get; set; }

        /// <summary>
        /// Utc time when the user plan to have this eat.
        /// </summary>
        public DateTime? EatUtcAt { get; set; }

        public BasicDishQtyRequest[] Dishes { get; set; }
        public BasicDishQtyRequest[] CompoundDishes { get; set; }
    }
}
