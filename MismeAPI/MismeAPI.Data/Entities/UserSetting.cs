using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("usersetting")]
    public class UserSetting
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int SettingId { get; set; }
        public Setting Setting { get; set; }
        public string Value { get; set; }
    }
}