using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("result")]
    public class Result
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string TextEN { get; set; }
        public string TextIT { get; set; }
        public string CodeName { get; set; }
        public string ConceptName { get; set; }
    }
}