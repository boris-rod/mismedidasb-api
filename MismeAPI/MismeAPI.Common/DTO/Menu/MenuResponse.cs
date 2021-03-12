using MismeAPI.Common.DTO.Group;
using MismeAPI.Common.DTO.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Menu
{
    public class MenuResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameEN { get; set; }
        public string NameIT { get; set; }
        public string Description { get; set; }
        public string DescriptionEN { get; set; }
        public string DescriptionIT { get; set; }
        public bool Active { get; set; }
        public int? GroupId { get; set; }
        public GroupResponse Group { get; set; }
        public int? CreatedById { get; set; }
        public UserSimpleResponse CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public ICollection<MenuEatResponse> Eats { get; set; }
    }
}
