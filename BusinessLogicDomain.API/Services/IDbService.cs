using BusinessLogicDomain.MarketDataDomainAPIClient;

namespace BusinessLogicDomain.API.Services
{
    public interface IDbService
    {
        Task InitializeCompanies(ICollection<StockSymbolDto> stockSymbols);
        Task SaveLiveDistinctMarketData(ICollection<MarketDataDto> marketData);
    }
}