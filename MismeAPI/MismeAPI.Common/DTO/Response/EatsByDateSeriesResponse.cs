using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response
{
    public class EatsByDateSeriesResponse
    {
        public string Name { get; set; }
        public List<BasicSerieResponse> Series { get; set; }
    }
}