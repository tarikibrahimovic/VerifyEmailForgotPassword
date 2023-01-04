using Microsoft.Extensions.Configuration;
using VerifyEmailForgotPassword.Data.Model;

namespace VerifyEmailForgotPassword.Data
{
    public class DataContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        public DataContext(DbContextOptions<DataContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            });
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<LinkVotes>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.NoAction);
    }

        public DbSet<User> Users => Set<User>();
        public DbSet<Favorites> Favorites => Set<Favorites>();
        public DbSet<User_Favorites> User_Favorites => Set<User_Favorites>();

        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Links> Link => Set<Links>();
        public DbSet<LinkVotes> Votes => Set<LinkVotes>();
    }
}
