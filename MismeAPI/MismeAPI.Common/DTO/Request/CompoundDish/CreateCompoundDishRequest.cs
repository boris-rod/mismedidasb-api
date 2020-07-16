using Microsoft.AspNetCore.Http;

namespace MismeAPI.Common.DTO.Request.CompoundDish
{
    public class CreateCompoundDishRequest
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }

        public BasicDishQtyRequest[] Dishes { get; set; }
    }
}
