using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("soloanswer")]
    public class SoloAnswer
    {
        public SoloAnswer()
        {
            UserSoloAnswers = new HashSet<UserSoloAnswer>();
        }

        public int Id { get; set; }
        public int SoloQuestionId { get; set; }
        public SoloQuestion SoloQuestion { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        public string TitleEN { get; set; }
        public string TitleIT { get; set; }
        public int Points { get; set; }

        public virtual ICollection<UserSoloAnswer> UserSoloAnswers { get; set; }
    }
}
