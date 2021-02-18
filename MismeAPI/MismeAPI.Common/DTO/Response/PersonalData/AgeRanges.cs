using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.PersonalData
{
    public class AgeRanges
    {
        public int CountRange18To24 { get; set; }
        public int PercentageRange18To24 { get; set; }
        public int CountRange25To34 { get; set; }
        public int PercentageRange25To34 { get; set; }
        public int CountRange35To44 { get; set; }
        public int PercentageRange35To44 { get; set; }
        public int CountRange45To54 { get; set; }
        public int PercentageRange45To54 { get; set; }
        public int CountRange55To64 { get; set; }
        public int PercentageRange55To64 { get; set; }
        public int CountRangeMin65 { get; set; }
        public int PercentageRangeMin65 { get; set; }
    }
}
