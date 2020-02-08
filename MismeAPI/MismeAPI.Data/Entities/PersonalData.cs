using MismeAPI.Data.Entities.Enums;
using System;

namespace MismeAPI.Data.Entities
{
    public class PersonalData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MeasureUnit { get; set; }
        public string CodeName { get; set; }
        public int Order { get; set; }
        public TypeEnum Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}