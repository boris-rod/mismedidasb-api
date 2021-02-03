using System.ComponentModel.DataAnnotations;

namespace MismeAPI.Common.DTO.Group
{
    public class CreateGroupRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        public string Description { get; set; }

        [EmailAddress]
        [Required(AllowEmptyStrings = false)]
        public string AdminEmail { get; set; }

        /// <summary>
        /// EN, ES, IT
        /// </summary>
        public string Language { get; set; }
    }
}
