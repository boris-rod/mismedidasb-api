using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("scheduledemail")]
    public class ScheduledEmail
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Emails { get; set; }
        public string Filepath { get; set; }
        public bool Sent { get; set; }
        public int RetryCount { get; set; }
        public string ExceptionMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
