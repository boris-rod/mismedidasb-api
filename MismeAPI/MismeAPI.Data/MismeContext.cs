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
        public DbSet<PersonalData> PersonalData { get; set; }
        public DbSet<UserPersonalData> UserPersonalData { get; set; }
        public DbSet<Poll> Poll { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Answer> Answer { get; set; }
        public DbSet<UserAnswer> UserAnswer { get; set; }
    }
}