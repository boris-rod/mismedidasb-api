using MismeAPI.Data.Entities.Enums;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("personaldata")]
    public class PersonalData
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public PersonalDataEnum Key { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
