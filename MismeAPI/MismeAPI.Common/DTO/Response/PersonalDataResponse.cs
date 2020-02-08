using System;

namespace MismeAPI.Common.DTO.Response
{
    public class PersonalDataResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MeasureUnit { get; set; }
        public string CodeName { get; set; }
        public int Order { get; set; }
        public string Type { get; set; }
        public int TypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}