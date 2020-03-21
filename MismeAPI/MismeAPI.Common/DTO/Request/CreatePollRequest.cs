using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO.Request
{
    public class CreatePollRequest
    {
        [Required]
        public int ConceptId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}