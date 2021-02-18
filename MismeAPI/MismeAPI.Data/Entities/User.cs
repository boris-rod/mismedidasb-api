using MismeAPI.Data.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            UserSchedules = new HashSet<UserSchedule>();
            Subscriptions = new HashSet<UserSubscription>();
            UserSoloAnswers = new HashSet<UserSoloAnswer>();
            FavoriteDishes = new HashSet<FavoriteDish>();
            LackSelfControlDishes = new HashSet<LackSelfControlDish>();
            Orders = new HashSet<Order>();
            FavoriteCompoundDishes = new HashSet<FavoriteCompoundDishes>();
            LackSelfControlCompoundDishes = new HashSet<LackSelfControlCompoundDish>();
            PersonalDatas = new HashSet<PersonalData>();
        }

        public int Id { get; set; }
        public string StripeCustomerId { get; set; }
        public string PaypalCustomerId { get; set; }

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
        public DateTime? LastAccessAt { get; set; }
        public bool MarkedForDeletion { get; set; }
        public int VerificationCode { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public string TimeZone { get; set; }
        public int TimeZoneOffset { get; set; }
        public double CurrentImc { get; set; }
        public double CurrentKcal { get; set; }
        public DateTime? FirtsHealthMeasured { get; set; }
        public int BreakFastKCalPercentage { get; set; }
        public int Snack1KCalPercentage { get; set; }
        public int LunchKCalPercentage { get; set; }
        public int Snack2KCalPercentage { get; set; }
        public int DinnerKCalPercentage { get; set; }
        public int Weight { get; set; }
        public int Height { get; set; }
        public int Age { get; set; }
        public UserStatistics UserStatistics { get; set; }
        public UserReferral Invitation { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }
        public virtual ICollection<UserToken> UserTokens { get; set; }
        public virtual ICollection<Eat> Eats { get; set; }
        public virtual ICollection<Device> Devices { get; set; }
        public virtual ICollection<UserConcept> UserConcepts { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
        public virtual ICollection<CompoundDish> CompoundDishs { get; set; }
        public virtual ICollection<RewardAcumulate> RewardAcumulates { get; set; }
        public virtual ICollection<UserReferral> Referrals { get; set; }
        public virtual ICollection<UserSchedule> UserSchedules { get; set; }
        public virtual ICollection<UserSubscription> Subscriptions { get; set; }
        public virtual ICollection<UserSoloAnswer> UserSoloAnswers { get; set; }
        public virtual ICollection<FavoriteDish> FavoriteDishes { get; set; }
        public virtual ICollection<LackSelfControlDish> LackSelfControlDishes { get; set; }
        public virtual ICollection<FavoriteCompoundDishes> FavoriteCompoundDishes { get; set; }
        public virtual ICollection<LackSelfControlCompoundDish> LackSelfControlCompoundDishes { get; set; }
        public virtual ICollection<GroupInvitation> GroupInvitations { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<PersonalData> PersonalDatas { get; set; }
    }
}
