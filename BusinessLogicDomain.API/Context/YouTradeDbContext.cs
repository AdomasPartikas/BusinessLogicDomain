using BusinessLogicDomain.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicDomain.API.Context.YouTradeDbContext
{
    public class YouTradeContext(DbContextOptions<YouTradeContext> options) : DbContext(options)
    {
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<LivePriceDaily> LivePriceDaily { get; set; } = null!;
        public DbSet<LivePriceDistinct> LivePriceDistinct { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserProfile> UserProfiles { get; set; } = null!;
        public DbSet<UserTransaction> UserTransactions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserProfile ↔ User (One-to-One)
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne()
                .HasForeignKey<UserProfile>(up => up.UserId) // Specify foreign key
                .OnDelete(DeleteBehavior.Cascade); // Optional: Deletes UserProfile if User is deleted

            // UserTransaction ↔ UserProfile (One-to-Many)
            modelBuilder.Entity<UserTransaction>()
                .HasOne(ut => ut.UserProfile)
                .WithMany(up => up.UserTransactions)
                .OnDelete(DeleteBehavior.Restrict);

            // PortfolioStock ↔ UserProfile (One-to-Many)
            modelBuilder.Entity<PortfolioStock>()
                .HasOne(ps => ps.UserProfile)
                .WithMany(up => up.UserPortfolioStocks)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}