using System;

namespace MismeAPI.Data.Entities
{
    public class UserPersonalData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int PersonalDataId { get; set; }
        public PersonalData PersonalData { get; set; }
        public string Value { get; set; }
        public DateTime MeasuredAt { get; set; }
    }
}