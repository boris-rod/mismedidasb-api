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
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<EatSchedule> EatSchedule { get; set; }
        public DbSet<UserSchedule> UserSchedule { get; set; }
        public DbSet<SoloAnswer> SoloAnswer { get; set; }
        public DbSet<SoloQuestion> SoloQuestion { get; set; }
        public DbSet<UserSoloAnswer> UserSoloAnswer { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<UserSubscription> UserSubscription { get; set; }
        public DbSet<UserSubscriptionSchedule> UserSubscriptionSchedule { get; set; }
        public DbSet<FavoriteDish> FavoriteDish { get; set; }
        public DbSet<FavoriteCompoundDishes> FavoriteCompoundDish { get; set; }
        public DbSet<App> App { get; set; }
        public DbSet<HandConversionFactor> HandConversionFactors { get; set; }
        public DbSet<LackSelfControlDish> LackSelfControlDishes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<GroupInvitation> GroupInvitation { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasOne(p => p.Invitation).WithOne(b => b.Invited).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<User>().HasMany(p => p.Referrals).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Eat>().HasOne(p => p.EatSchedule).WithOne(b => b.Eat).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Schedule>().HasMany(p => p.EatSchedules).WithOne(b => b.Schedule).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(p => p.UserSchedules).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Schedule>().HasMany(p => p.UserSchedules).WithOne(b => b.Schedule).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SoloAnswer>().HasMany(p => p.UserSoloAnswers).WithOne(b => b.SoloAnswer).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>().HasMany(p => p.FavoriteDishes).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Dish>().HasMany(p => p.FavoriteDishes).WithOne(b => b.Dish).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(p => p.LackSelfControlDishes).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Dish>().HasMany(p => p.LackSelfControlDishes).WithOne(b => b.Dish).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().HasMany(p => p.FavoriteCompoundDishes).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CompoundDish>().HasMany(p => p.FavoriteDishes).WithOne(b => b.Dish).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>().HasMany(p => p.LackSelfControlDishes).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CompoundDish>().HasMany(p => p.LackSelfControlDishes).WithOne(b => b.Dish).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>().Property(p => p.BreakFastKCalPercentage).HasDefaultValue(20);
            modelBuilder.Entity<User>().Property(p => p.Snack1KCalPercentage).HasDefaultValue(10);
            modelBuilder.Entity<User>().Property(p => p.LunchKCalPercentage).HasDefaultValue(35);
            modelBuilder.Entity<User>().Property(p => p.Snack2KCalPercentage).HasDefaultValue(10);
            modelBuilder.Entity<User>().Property(p => p.DinnerKCalPercentage).HasDefaultValue(25);

            modelBuilder.Entity<User>().HasMany(p => p.Orders).WithOne(b => b.User).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Product>().HasMany(p => p.Orders).WithOne(b => b.Product).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>().HasMany(p => p.GroupInvitations).WithOne(b => b.User).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Group>().HasMany(p => p.Invitations).WithOne(b => b.Group).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
