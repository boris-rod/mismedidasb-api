using Microsoft.AspNetCore.Http;

namespace MismeAPI.Common.DTO.Request.Concept
{
    public class AddConceptRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
    }
}