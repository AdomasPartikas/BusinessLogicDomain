using BusinessLogicDomain.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicDomain.API.Context.YouTradeDbContext
{
    public class YouTradeContext(DbContextOptions<YouTradeContext> options) : DbContext(options)
    {
        public DbSet<BuyOrder> BuyOrders { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<SellOrder> SellOrders { get; set; }
        public DbSet<LivePriceDaily> LivePriceDaily { get; set; }
        public DbSet<LivePriceDistinct> LivePriceDistinct { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserTransactions> UserTransactions { get; set; }
    }
}