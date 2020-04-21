using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("reminder")]
    public class Reminder
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string CronExpression { get; set; }
        public string CodeName { get; set; }
    }
}