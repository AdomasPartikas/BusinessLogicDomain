using AutoMapper;
using BusinessLogicDomain.API.Context.YouTradeDbContext;
using BusinessLogicDomain.API.Groups;
using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Microsoft.EntityFrameworkCore;
using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.API.Entities.Enum;

namespace BusinessLogicDomain.API.Services
{
    public class DbService(YouTradeContext context, IMapper mapper) : IDbService
    {
        public readonly YouTradeContext _context = context;
        public readonly IMapper _mapper = mapper;

        public async Task InitializeCompanies(ICollection<StockSymbolDto> stockSymbols)
        {
            foreach (var symbol in stockSymbols)
            {
                var existingCompany = await _context.Companies.FirstOrDefaultAsync(c => c.ID == symbol.DisplaySymbol);

                var mapped = _mapper.Map<Company>(symbol);

                if (existingCompany != null)
                {
                    existingCompany.Name = mapped.Name;
                    existingCompany.ID = mapped.ID;
                }
                else
                {
                    await _context.Companies.AddAsync(mapped);
                }
            }

            await _context.SaveChangesAsync();
        }

    /*    public async Task UpdateLiveDistinctMarketData(ICollection<MarketDataDto> marketData) //Naudojamas Kokybei Lauros
        {
            if(_context.LivePriceDistinct.Any())
                await UpdateLiveDailyMarketData(_context.LivePriceDistinct.ToList());

            var livePriceDistincts = marketData.Select(data => _mapper.Map<LivePriceDistinct>(data)).ToList();

            foreach(var record in livePriceDistincts)
            {
                var existingRecord = await _context.LivePriceDistinct.FirstOrDefaultAsync(l => l.ID == record.ID);

                if (existingRecord != null)
                {
                    existingRecord.Price = record.Price;
                    existingRecord.Date = record.Date;
                }
                else
                    await _context.LivePriceDistinct.AddAsync(record);
            }

            await _context.SaveChangesAsync();
        }*/
    public async Task UpdateLiveDistinctMarketData(ICollection<MarketDataDto> marketData)
    {

        if (marketData == null || marketData.Count() == 0)
            throw new ArgumentException("Market data cannot be null or empty.");

        if (_context.LivePriceDistinct.Any())
            await UpdateLiveDailyMarketData(_context.LivePriceDistinct.ToList());

        var livePriceDistincts = marketData
            .Select(data => _mapper.Map<LivePriceDistinct>(data))
            .ToList();

        foreach (var record in livePriceDistincts)
        {
            var existingRecord = await GetExistingRecordAsync(record.ID);

            if (existingRecord != null)
            {
                await UpdateExistingRecord(existingRecord, record);
            }
            else
            {
                await AddNewRecord(record);
            }
        }

        await _context.SaveChangesAsync();
    }


    private async Task<LivePriceDistinct?> GetExistingRecordAsync(string id)
    {
        return await _context.LivePriceDistinct.FirstOrDefaultAsync(l => l.ID == id);
    }

    // Atnaujinti esamą įrašą
    private static Task UpdateExistingRecord(LivePriceDistinct existingRecord, LivePriceDistinct newRecord)
    {
        existingRecord.Price = newRecord.Price;
        existingRecord.Date = newRecord.Date;

        return Task.CompletedTask;
    }

    // Pridėti naują įrašą
    private async Task AddNewRecord(LivePriceDistinct newRecord)
    {
        await _context.LivePriceDistinct.AddAsync(newRecord);
    }

        /*
                public async Task UpdatePriceHistory() //Kokybei Patriko
                {
                    var livePriceDailies = await _context.LivePriceDaily
                    .GroupBy(l => l.Company.ID)
                    .Select(g => g.OrderByDescending(l => l.Date).FirstOrDefault())
                    .ToListAsync();

                    if(livePriceDailies.Count == 0)
                        return; //TODO: Log reason for return

                    var priceHistories = livePriceDailies.Select(data => _mapper.Map<PriceHistory>(data)).ToList();

                    foreach (var priceHistory in priceHistories.ToList())
                    {
                        if (!await IsPriceHistoryDistinct(_mapper.Map<PriceHistoryDto>(priceHistory)))
                            priceHistories.Remove(priceHistory);
                    }

                    await _context.PriceHistories.AddRangeAsync(priceHistories);
                    await _context.SaveChangesAsync();

                    //await RemovePriceHistoryDuplicates(); //Uncomment to remove duplicates (for testing purposes)

                    await ClearLivePriceDaily();
                }
        */

        public async Task UpdatePriceHistory() //Kokybei Patriko
        {
            var livePriceDailies = await GetLatestLivePriceDailyEntriesAsync();

            if(livePriceDailies.Count == 0)
                return;

            var distinctPriceHistories = await GetDistinctPriceHistoriesAsync(livePriceDailies);

            if (distinctPriceHistories.Count != 0)
                await SavePriceHistoriesAsync(distinctPriceHistories);

            await ClearLivePriceDaily();
        }
        
        private async Task<List<LivePriceDaily>> GetLatestLivePriceDailyEntriesAsync()
        {
            var latestLivePrice = await _context.LivePriceDaily
                .GroupBy(l => l.Company.ID)
                .Select(g => g.OrderByDescending(l => l.Date).FirstOrDefault())
                .ToListAsync();

            if(latestLivePrice == null)
                return [];

            return latestLivePrice!;
        }

        private async Task<List<PriceHistory>> GetDistinctPriceHistoriesAsync(List<LivePriceDaily> livePriceDailies)
        {
            var priceHistories = livePriceDailies
                .Select(data => _mapper.Map<PriceHistory>(data))
                .ToList();

            var distinctPriceHistories = new List<PriceHistory>();

            foreach (var priceHistory in priceHistories)
            {
                if (await IsPriceHistoryDistinct(_mapper.Map<PriceHistoryDto>(priceHistory)))
                    distinctPriceHistories.Add(priceHistory);
            }

            return distinctPriceHistories;
        }

        private async Task SavePriceHistoriesAsync(List<PriceHistory> priceHistories)
        {
            await _context.PriceHistories.AddRangeAsync(priceHistories);
            await _context.SaveChangesAsync();
        }

        private async Task<bool> IsPriceHistoryDistinct(PriceHistoryDto priceHistory)
        {
            var existingPriceHistory = await _context.PriceHistories
                .FirstOrDefaultAsync(ph => ph.Company.ID == priceHistory.CompanySymbol && ph.Date == priceHistory.Date && ph.EODPrice == priceHistory.EODPrice);

            return existingPriceHistory == null;
        }

        private async Task RemovePriceHistoryDuplicates()
        {
            var duplicateGroups = _context.PriceHistories
                .AsEnumerable()
                .GroupBy(ph => new { ph.Date, ph.EODPrice, ph.Company.ID })
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in duplicateGroups)
            {
                var duplicates = group.Skip(1).ToList();
                _context.PriceHistories.RemoveRange(duplicates);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ClearLivePriceDaily()
        {
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE LivePriceDaily");
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

        public async Task<UserProfile> CreateUserProfile(User newUser, decimal balance, SimulationLevel simulationLevel)
        {
            var userProfile = new UserProfile
            {
                User = newUser,
                Balance = balance,
                SimulationLevel = simulationLevel,
                UserTransactions = [],
                UserPortfolioStocks = []
            };

            await _context.UserProfiles.AddAsync(userProfile);
            await _context.SaveChangesAsync();

            return userProfile;
        }

        public async Task<User> CreateUser(UserRegisterDto newUser)
        {
            var user = new User
            {
                UserName = newUser.UserName,
                Password = newUser.Password,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                DateOfBirth = newUser.DateOfBirth,
                Email = newUser.Email,
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await CreateUserProfile(user, newUser.Balance, newUser.SimulationLevel);

            return user;
        }

        public async Task<User?> RetrieveUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == id);

            return user;
        }

        public async Task<User?> RetrieveUserByUsername(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

            return user;
        }

        public async Task<User?> RetrieveUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            return user;
        }

        public async Task<UserProfile?> RetrieveUserProfile(int id)
        {
            var userProfile = await _context.UserProfiles
                .Include(up => up.User)
                .Include(up => up.UserTransactions)
                .Include(up => up.UserPortfolioStocks)
                .AsSplitQuery()
                .FirstOrDefaultAsync(up => up.User.ID == id);

            return userProfile;
        }

        public async Task<UserProfile> UpdateUserProfile(UserProfile userProfile)
        {
            _context.UserProfiles.Update(userProfile);
            await _context.SaveChangesAsync();

            return userProfile;
        }

        public async Task UpdateUser(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetCurrentStockPriceOfCompany(string symbol)
        {
            var company = await RetrieveCompanyBySymbol(symbol);
            var livePriceDistinct = await GetCompanyLivePriceDistinct(symbol);

            return livePriceDistinct.Price;
        }

        public async Task UpdateTransaction(UserTransaction transaction)
        {
            _context.UserTransactions.Update(transaction);

            await _context.SaveChangesAsync();
        }

        public async Task<List<UserProfile>> RetrieveAllUserProfiles()
        {
            var userProfiles = await _context.UserProfiles
                .Include(up => up.User)
                .Include(up => up.UserTransactions)
                .Include(up => up.UserPortfolioStocks)
                .AsSplitQuery()
                .ToListAsync();

            return userProfiles;
        }

        public async Task<UserTransaction> RetrieveUserTransaction(int id)
        {
            var existingTransaction = await _context.UserTransactions.FirstOrDefaultAsync(t => t.ID == id);

            return existingTransaction;
        }

    }
}