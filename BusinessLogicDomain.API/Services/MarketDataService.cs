using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using OfficeOpenXml;

namespace BusinessLogicDomain.API.Services
{
    public class MarketDataService(MarketDataDomainClient marketDataClient, IDbService dbService) : IMarketDataService
    {
        private readonly MarketDataDomainClient _marketDataClient = marketDataClient;
        private readonly IDbService _dbService = dbService;
        public async Task RetrieveAndSaveMarketData()
        {
            var marketData = await _marketDataClient.MarketdataAsync();

            if(marketData == null)
                return; //TODO: Log error

            await _dbService.UpdateLiveDistinctMarketData(marketData);
        }

        public async Task RetrieveAndSaveAvailableCompanies()
        {
            var stockSymbols = await _marketDataClient.StocksymbolsAsync();

            if(stockSymbols == null)
                return; //TODO: Log error

            await _dbService.InitializeCompanies(stockSymbols);
        }

        private async Task UpdateTablesForMarketClosure()
        {
            await RetrieveAndSaveMarketData();

            await _dbService.UpdatePriceHistory();
        }

        public async Task RefreshMarketData()
        {
            var marketStatus = await _marketDataClient.MarketstatusAsync();

            if(marketStatus == null)
            {
                Console.WriteLine("Market status is null");
                return; //TODO: Log error
            }

            if(marketStatus.IsOpen)
                await RetrieveAndSaveMarketData();
            else
                await UpdateTablesForMarketClosure();
        }
    }
}