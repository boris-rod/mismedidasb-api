using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.CutPoints
{
    public class CreateCutPointRequest
    {
        public int Points { get; set; }
        public string Description { get; set; }
    }
}
