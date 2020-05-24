using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MismeAPI.Common.DTO.Request
{
    public class CreateUserReferralRequest
    {
        [EmailAddress]
        [Required(AllowEmptyStrings = false)]
        public string Email { get; set; }
    }
}
