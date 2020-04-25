using MismeAPI.Data.Entities.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("generalcontent")]
    public class GeneralContent
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string ContentEN { get; set; }
        public string ContentIT { get; set; }
        public ContentTypeEnum ContentType { get; set; }
    }
}