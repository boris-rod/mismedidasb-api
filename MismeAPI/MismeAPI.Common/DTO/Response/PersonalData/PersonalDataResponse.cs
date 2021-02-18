using System;

namespace MismeAPI.Common.DTO.Response.PersonalData
{
    public class PersonalDataResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int KeyId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
