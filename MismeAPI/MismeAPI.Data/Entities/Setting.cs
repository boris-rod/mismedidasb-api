using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("setting")]
    public class Setting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}