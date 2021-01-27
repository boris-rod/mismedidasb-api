using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MismeAPI.Common.DTO.Group
{
    public class AdminUpdateGroupRequest
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
