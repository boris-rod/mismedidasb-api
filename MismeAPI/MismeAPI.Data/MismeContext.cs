using Microsoft.EntityFrameworkCore;
using MismeAPI.Data.Entities;

namespace MismeAPI.Data
{
    public class MismeContext : DbContext
    {
        public MismeContext(DbContextOptions<MismeContext> options) : base(options)
        {
        }

        public DbSet<User> User { get; set; }
        public DbSet<UserToken> UserToken { get; set; }

        public DbSet<Poll> Poll { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Answer> Answer { get; set; }
        public DbSet<UserAnswer> UserAnswer { get; set; }
        public DbSet<Concept> Concept { get; set; }
        public DbSet<UserConcept> UserConcept { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<DishTag> DishesTags { get; set; }
        public DbSet<Eat> Eats { get; set; }
        public DbSet<EatDish> EatDishes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Tip> Tips { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<GeneralContent> GeneralContents { get; set; }
        public DbSet<ContactUs> CotactUs { get; set; }
        public DbSet<RewardCategory> RewardCategory { get; set; }
        public DbSet<RewardAcumulate> RewardAcumulate { get; set; }
        public DbSet<RewardHistory> RewardHistory { get; set; }
        public DbSet<UserStatistics> UserStatistics { get; set; }
        public DbSet<DishCompoundDish> DishCompoundDishes { get; set; }
        public DbSet<CompoundDish> CompoundDishes { get; set; }
        public DbSet<CutPoint> CutPoint { get; set; }
        public DbSet<UserReferral> UserReferral { get; set; }
        public DbSet<EatCompoundDish> EatCompoundDishes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasOne(p => p.Invitation).WithOne(b => b.Invited).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<User>().HasMany(p => p.Referrals).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);
        }
    }
}