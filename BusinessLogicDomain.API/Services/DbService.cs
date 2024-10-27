using AutoMapper;
using BusinessLogicDomain.API.Context.YouTradeDbContext;
using BusinessLogicDomain.API.Groups;
using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Microsoft.EntityFrameworkCore;
using BusinessLogicDomain.API.Models;

namespace BusinessLogicDomain.API.Services
{
    public class DbService(YouTradeContext context, IMapper mapper) : IDbService
    {
        public readonly YouTradeContext _context = context;
        public readonly IMapper _mapper = mapper;

        public async Task InitializeCompanies(ICollection<StockSymbolDto> stockSymbols)
        {
            _context.Companies.RemoveRange(_context.Companies);
            await _context.SaveChangesAsync();

            var companies = stockSymbols.Select(symbol => _mapper.Map<Company>(symbol)).ToList();
            await _context.Companies.AddRangeAsync(companies);
            await _context.SaveChangesAsync();
        }

        public async Task RefreshLiveDistinctMarketData(ICollection<MarketDataDto> marketData)
        {
            if(_context.LivePriceDistinct.Any())
                await UpdateLiveDailyMarketData(_context.LivePriceDistinct.ToList());

            await ClearLivePriceDistinct();

            var livePriceDistincts = marketData.Select(data => _mapper.Map<LivePriceDistinct>(data)).ToList();
            await _context.LivePriceDistinct.AddRangeAsync(livePriceDistincts);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePriceHistory()
        {
            await ClearLivePriceDistinct();

            var livePriceDailies = await _context.LivePriceDaily
            .GroupBy(l => l.Company.ID)
            .Select(g => g.OrderByDescending(l => l.Date).FirstOrDefault())
            .ToListAsync();

            if(livePriceDailies.Count == 0)
                return; //TODO: Log reason for return

            var priceHistories = livePriceDailies.Select(data => _mapper.Map<PriceHistory>(data)).ToList();

            await _context.PriceHistories.AddRangeAsync(priceHistories);
            await _context.SaveChangesAsync();

            await ClearLivePriceDaily();
        }

        private async Task ClearLivePriceDistinct()
        {
            _context.LivePriceDistinct.RemoveRange(_context.LivePriceDistinct);
            await _context.SaveChangesAsync();
        }

        private async Task ClearLivePriceDaily()
        {
            _context.LivePriceDaily.RemoveRange(_context.LivePriceDaily);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLiveDailyMarketData(ICollection<LivePriceDistinct> distinctMarketData)
        {
            var livePriceDailies = new List<LivePriceDaily>();

            foreach (var data in distinctMarketData)
            {
                var companyWithPriceDistinct = new CompanyWithPriceDistinct
                {
                    Company = await RetrieveCompanyBySymbol(data.ID),
                    LivePriceDistinct = data
                };
                livePriceDailies.Add(_mapper.Map<LivePriceDaily>(companyWithPriceDistinct));
            }

            await _context.LivePriceDaily.AddRangeAsync(livePriceDailies);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Company>> RetrieveInitializedCompanies()
        {
            var companies = await _context.Companies.ToListAsync();

            return companies;
        }

        public async Task<Company> RetrieveCompanyBySymbol(string symbol)
        {
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.ID == symbol);

            return company!;
        }

        public async Task<LivePriceDistinct> GetCompanyLivePriceDistinct(string symbol)
        {
            var livePriceDistinct = await _context.LivePriceDistinct.FirstOrDefaultAsync(l => l.ID == symbol);

            return livePriceDistinct!;
        }

        public async Task<List<LivePriceDaily>> GetCompanyLivePriceDaily(string symbol)
        {
            var priceHistory = await _context.LivePriceDaily
            .Where(l => l.Company.ID == symbol)
            .ToListAsync();

            return priceHistory;
        }

        public async Task<List<PriceHistory>> GetCompanyPriceHistory(string symbol, DateTime startDate, DateTime endDate)
        {
            return await _context.PriceHistories
                .Where(ph => ph.Company.ID == symbol && ph.Date >= startDate && ph.Date <= endDate)
                .ToListAsync();
        }

        public async Task<UserProfile> CreateUserProfile(User newUser, decimal balance)
        {
            var userProfile = new UserProfile
            {
                User = newUser,
                Balance = balance,
                UserTransactions = [],
                BuyOrders = [],
                SellOrders = []
            };

            await _context.UserProfiles.AddAsync(userProfile);
            await _context.SaveChangesAsync();

            return userProfile;
        }

        public async Task<User> CreateUser(UserDTO newUser)
        {
            var user = new User
            {
                UserName = newUser.UserName,
                Password = newUser.Password,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                DateOfBirth = newUser.DateOfBirth,
                Address = newUser.Address,
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await CreateUserProfile(user, newUser.Balance);

            return user;
        }

        public async Task<User?> RetrieveUser(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            return user;
        }

        public async Task<UserProfile> RetrieveUserProfile(string userName)
        {
            var userProfile = await _context.UserProfiles
                .Include(up => up.User)
                .Include(up => up.UserTransactions)
                .Include(up => up.BuyOrders)
                .Include(up => up.SellOrders)
                .FirstOrDefaultAsync(up => up.User.UserName == userName);

            return userProfile!;
        }

        public async Task UpdateUserProfile(UserProfile userProfile)
        {
            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}