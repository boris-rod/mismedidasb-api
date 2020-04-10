using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Request
{
    public class CreateBulkEatRequest
    {
        public DateTime DateInUtc { get; set; }
        public List<CreateEatRequest> Eats { get; set; }
    }
}