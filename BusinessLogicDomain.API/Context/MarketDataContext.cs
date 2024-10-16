using BusinessLogicDomain.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogicDomain.MarketDataDbContext
{
    public class MarketDataContext : DbContext
    {
        public MarketDataContext(DbContextOptions<MarketDataContext> options) : base(options)
        {
        }

        public DbSet<BuyOrder> BuyOrders { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<SellOrder> SellOrders { get; set; }
        public DbSet<TempDayPrice> TempDayPrices { get; set; }
        public DbSet<TempHourPrice> TempHourPrices { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserTransactions> UserTransactions { get; set; }
    }
}