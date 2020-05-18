using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request
{
    public class CreateBulkEatRequest
    {
        public DateTime DateInUtc { get; set; }

        /// <summary>
        /// says if the plan is balanced or not.
        /// </summary>
        public bool IsBalanced { get; set; }

        public List<CreateEatRequest> Eats { get; set; }
    }
}
