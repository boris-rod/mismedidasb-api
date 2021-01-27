using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO.Group
{
    public class UpdateGroupRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
