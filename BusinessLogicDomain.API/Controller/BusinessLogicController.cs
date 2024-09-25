using BusinessLogicDomain.MarketDataDomainAPIClient;
using Microsoft.AspNetCore.Mvc;

namespace BusinessLogicDomain.API.Controller
{
    [Route("api")]
    [ApiController]
    public class BusinessLogicController(MarketDataDomainClient marketDataClient) : ControllerBase
    {
        private readonly MarketDataDomainClient _marketDataClient = marketDataClient;

        [HttpGet("marketdata/getstockdata")]
        public async Task<IActionResult> GetMarketData()
        {
            var marketData = await _marketDataClient.MarketdataAsync();
            return Ok(marketData);
        }
        
        [HttpGet("marketdata/getavailablestocks")]
        public async Task<IActionResult> GetAvailableStocks()
        {
            var marketData = await _marketDataClient.StocksymbolsAsync();
            return Ok(marketData);
        }
    }
}