using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("soloquestion")]
    public class SoloQuestion
    {
        public SoloQuestion()
        {
            SoloAnswers = new HashSet<SoloAnswer>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string TitleEN { get; set; }
        public string TitleIT { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public bool AllowCustomAnswer { get; set; }
        public virtual ICollection<SoloAnswer> SoloAnswers { get; set; }
    }
}
