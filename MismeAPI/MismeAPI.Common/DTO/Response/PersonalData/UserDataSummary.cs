using System;
using System.Collections.Generic;
using System.Text;

namespace MismeAPI.Common.DTO.Response.PersonalData
{
    public class UserDataSummary
    {
        public AgeRanges AgeRange { get; set; }
        public int TotalUsers { get; set; }
        public int TotalActiveUsers { get; set; }
    }
}
