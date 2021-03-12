using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Menu
{
    public class CreateUpdateMenuRequest
    {
        public string Name { get; set; }
        public string NameEN { get; set; }
        public string NameIT { get; set; }
        public string Description { get; set; }
        public string DescriptionEN { get; set; }
        public string DescriptionIT { get; set; }
        public int? GroupId { get; set; }
    }
}
