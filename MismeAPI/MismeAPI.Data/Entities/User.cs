﻿using MismeAPI.Data.Entities.Enums;
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
            CompoundDishs = new HashSet<CompoundDish>();
            RewardAcumulates = new HashSet<RewardAcumulate>();
            Referrals = new HashSet<UserReferral>();
        }

        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public RoleEnum Role { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }

        public string Phone { get; set; }

        public StatusEnum Status { get; set; }

        public DateTime? LastLoggedIn { get; set; }
        public string Avatar { get; set; }
        public string AvatarMimeType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? DisabledAt { get; set; }
        public bool MarkedForDeletion { get; set; }
        public int VerificationCode { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public UserStatistics UserStatistics { get; set; }
        public UserReferral Invitation { get; set; }
        public virtual ICollection<UserToken> UserTokens { get; set; }
        public virtual ICollection<Eat> Eats { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        public virtual ICollection<UserConcept> UserConcepts { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
        public virtual ICollection<CompoundDish> CompoundDishs { get; set; }
        public virtual ICollection<RewardAcumulate> RewardAcumulates { get; set; }
        public virtual ICollection<UserReferral> Referrals { get; set; }
    }
}
