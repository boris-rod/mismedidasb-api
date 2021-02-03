using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO
{
    public class EmailRequest
    {
        [EmailAddress]
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
    }
}
