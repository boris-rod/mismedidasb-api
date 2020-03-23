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
        public DbSet<UserPoll> UserPoll { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<DishTag> DishesTags { get; set; }
        public DbSet<Eat> Eats { get; set; }
        public DbSet<EatDish> EatDishes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Tip> Tips { get; set; }
    }
}