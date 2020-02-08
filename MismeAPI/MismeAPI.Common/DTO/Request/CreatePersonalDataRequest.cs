using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO.Request
{
    public class CreatePersonalDataRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string MeasureUnit { get; set; }

        [Required]
        public string CodeName { get; set; }

        public int Order { get; set; }
        public int Type { get; set; }
    }
}