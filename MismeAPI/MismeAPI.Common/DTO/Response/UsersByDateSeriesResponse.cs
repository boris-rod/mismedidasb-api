using System.Collections.Generic;

namespace MismeAPI.Common.DTO.Response
{
    public class UsersByDateSeriesResponse
    {
        public string Name { get; set; }
        public List<BasicSerieResponse> Series { get; set; }
    }
}