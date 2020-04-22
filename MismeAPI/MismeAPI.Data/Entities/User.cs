using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MismeAPI.Data.Entities
{
    [Table("user")]
    public class User
    {
        public User()
        {
            UserTokens = new HashSet<UserToken>();
            Eats = new HashSet<Eat>();
            Devices = new HashSet<Device>();
            UserConcepts = new HashSet<UserConcept>();
            UserSettings = new HashSet<UserSetting>();
        }

        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public RoleEnum Role { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public StatusEnum Status { get; set; }

        public DateTime? LastLoggedIn { get; set; }
        public string Avatar { get; set; }
        public string AvatarMimeType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? DisabledAt { get; set; }

        public int VerificationCode { get; set; }
        public virtual ICollection<UserToken> UserTokens { get; set; }
        public virtual ICollection<Eat> Eats { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        public virtual ICollection<UserConcept> UserConcepts { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
    }
}