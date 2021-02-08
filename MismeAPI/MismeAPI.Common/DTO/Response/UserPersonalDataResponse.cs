using System;

namespace MismeAPI.Common.DTO.Response
{
    public class UserPersonalDataResponse
    {
        public int Id { get; set; }
        public PersonalDataResponse PersonalData { get; set; }
        public string Value { get; set; }
        public DateTime MeasuredAt { get; set; }
        public DateTime? LastAccessAt { get; set; }
    }
}
