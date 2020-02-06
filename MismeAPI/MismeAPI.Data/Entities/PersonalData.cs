using System;

namespace MismeAPI.Data.Entities
{
    public class PersonalData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MeasureUnit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}