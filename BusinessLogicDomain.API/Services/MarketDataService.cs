using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using OfficeOpenXml;

namespace BusinessLogicDomain.API.Services
{
    public class MarketDataService(MarketDataDomainClient marketDataClient, IDbService dbService) : IMarketDataService
    {
        private readonly MarketDataDomainClient _marketDataClient = marketDataClient;
        private readonly IDbService _dbService = dbService;
        public async Task RetrieveMarketData()
        {
            var marketData = await _marketDataClient.MarketdataAsync();

            if(marketData == null)
                return; //TODO: Log error

            await _dbService.SaveLiveDistinctMarketData(marketData);
        }

        public async Task RetrieveAndSaveAvailableStocks()
        {
            var stockSymbols = await _marketDataClient.StocksymbolsAsync();

            if(stockSymbols == null)
                return; //TODO: Log error

            await _dbService.InitializeCompanies(stockSymbols);
        }

        public async Task<bool> RetrieveMarketStatus()
        {
            var marketStatus = await _marketDataClient.MarketstatusAsync();

            if(marketStatus == null)
                return false; //TODO: Log error

            Console.WriteLine($"[{marketStatus.Timestamp}] Market is open: {marketStatus.IsOpen}");

            return marketStatus.IsOpen;
        }
    }
}