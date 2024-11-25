using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Entities.Enum;
using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.MarketDataDomainAPIClient;

namespace BusinessLogicDomain.API.Services
{
    public interface IDbService
    {
        Task InitializeCompanies(ICollection<StockSymbolDto> stockSymbols);
        Task UpdateLiveDistinctMarketData(ICollection<MarketDataDto> marketData);
        Task<List<Company>> RetrieveInitializedCompanies();
        Task<Company> RetrieveCompanyBySymbol(string symbol);
        Task UpdatePriceHistory();
        Task<LivePriceDistinct> GetCompanyLivePriceDistinct(string symbol);
        Task<List<LivePriceDaily>> GetCompanyLivePriceDaily(string symbol);
        Task<List<PriceHistory>> GetCompanyPriceHistory(string symbol, DateTime startDate, DateTime endDate);
        Task<User?> RetrieveUser(int id);
        Task<User?> RetrieveUserByUsername(string username);
        Task<User?> RetrieveUserByEmail(string email);
        Task<User> CreateUser(UserRegisterDTO newUser);
        Task<UserProfile> CreateUserProfile(User newUser, decimal balance, SimulationLevel simulationLevel);
        Task UpdateUser(User user);
        Task<UserProfile?> RetrieveUserProfile(int id);
        Task<UserProfile> UpdateUserProfile(UserProfile existingUserProfile);
        Task<decimal> GetCurrentStockPriceOfCompany(string symbol);
        Task UpdateTransaction(UserTransaction transaction);
        Task<List<UserProfile>> RetrieveAllUserProfiles();
        Task<UserTransaction> RetrieveUserTransaction(int id);
    }
}