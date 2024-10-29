using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.MarketDataDomainAPIClient;

namespace BusinessLogicDomain.API.Services
{
    public interface IDbService
    {
        Task InitializeCompanies(ICollection<StockSymbolDto> stockSymbols);
        Task RefreshLiveDistinctMarketData(ICollection<MarketDataDto> marketData);
        Task<List<Company>> RetrieveInitializedCompanies();
        Task<Company> RetrieveCompanyBySymbol(string symbol);
        Task UpdatePriceHistory();
        Task<LivePriceDistinct> GetCompanyLivePriceDistinct(string symbol);
        Task<List<LivePriceDaily>> GetCompanyLivePriceDaily(string symbol);
        Task<List<PriceHistory>> GetCompanyPriceHistory(string symbol, DateTime startDate, DateTime endDate);
        Task<User?> RetrieveUser(int id);
        Task<User?> RetrieveUserByUsername(string username);
        Task<User> CreateUser(UserRegisterDTO newUser);
        Task<UserProfile> CreateUserProfile(User newUser, decimal balance);
        Task UpdateUser(User user);
        Task<UserProfile?> RetrieveUserProfile(int id);
        Task<UserProfile> UpdateUserProfile(UserProfile existingUserProfile);
    }
}