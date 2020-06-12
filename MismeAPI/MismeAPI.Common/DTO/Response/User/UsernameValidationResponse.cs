using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.User
{
    public class UsernameValidationResponse
    {
        /// <summary>
        /// Is available or not the given username
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Suggestions generated from fullname and email
        /// </summary>
        public IEnumerable<string> Suggestions { get; set; }
    }
}
