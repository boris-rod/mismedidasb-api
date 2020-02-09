using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO.Request
{
    public class CreatePollRequest
    {
        [Required]
        public int ConceptId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Codename { get; set; }

        public string Description { get; set; }
    }
}