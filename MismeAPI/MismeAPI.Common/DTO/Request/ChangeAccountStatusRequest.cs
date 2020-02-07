using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO.Request
{
    public class ChangeAccountStatusRequest
    {
        [Required]
        public bool Active { get; set; }
    }
}