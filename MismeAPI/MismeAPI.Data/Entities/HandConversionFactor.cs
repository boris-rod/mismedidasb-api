using MismeAPI.Data.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("handconversionfactor")]
    public class HandConversionFactor
    {
        public int Id { get; set; }
        public GenderEnum Gender { get; set; }
        public int Height { get; set; }
        public double ConversionFactor { get; set; }
        public double ConversionFactor3Code { get; set; }
        public double ConversionFactor6Code { get; set; }
        public double ConversionFactor10Code { get; set; }
        public double ConversionFactor11Code { get; set; }
        public double ConversionFactor19Code { get; set; }
        public double ConversionFactor4Code { get; set; }
    }
}