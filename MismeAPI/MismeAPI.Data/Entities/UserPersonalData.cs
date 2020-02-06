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
        public DateTime CreatedAt { get; set; }
    }
}