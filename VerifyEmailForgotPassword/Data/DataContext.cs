

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
            //modelBuilder.Entity<User_Favorites>()
            //    .HasOne(u => u.User)
            //    .WithMany(u => u.User_Favorites)
            //    .HasForeignKey(bi => bi.UserId);

            //modelBuilder.Entity<User_Favorites>()
            //    .HasOne(f => f.Favorite)
            //    .WithMany(f => f.User_Favorites)
            //    .HasForeignKey(f => f.FavoriteId);

            modelBuilder.Entity<LinkVotes>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //ovo je za 2 dela tabele, definisanje knjiga i autora
    }

        public DbSet<User> Users => Set<User>();
        public DbSet<Favorites> Favorites => Set<Favorites>();
        public DbSet<User_Favorites> User_Favorites => Set<User_Favorites>();

        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Links> Link => Set<Links>();
        public DbSet<LinkVotes> Votes => Set<LinkVotes>();
    }
}
