using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Request.CutPoints
{
    public class UpdateCutPointRequest
    {
        public int Points { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
