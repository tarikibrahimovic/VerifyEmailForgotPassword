

using VerifyEmailForgotPassword.Data.Model;

namespace VerifyEmailForgotPassword.Data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=userdb;Trusted_Connection=true;");
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
