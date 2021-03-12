using System;
using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Menu
{
    public class MenuBulkEatsUpdateRequest
    {
        public List<CreateMenuEatRequest> Eats { get; set; }
    }
}
