using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.CutPoint
{
    public class CutPointResponse
    {
        public int Id { get; set; }
        public int Points { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
