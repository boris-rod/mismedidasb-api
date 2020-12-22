using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("app")]
    public class App
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public bool IsMandatory { get; set; }
        public string VersionIOS { get; set; }
        public bool IsMandatoryIOS { get; set; }
    }
}