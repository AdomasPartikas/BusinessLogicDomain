using Moq;
using Xunit;
using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogicDomain.Tests.Unit;

public class MarketDataServiceTests
{
    private readonly Mock<IMarketDataDomainClient> _marketDataClientMock;
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly MarketDataService _marketDataService;

    public MarketDataServiceTests()
    {
        _marketDataClientMock = new Mock<IMarketDataDomainClient>();
        _dbServiceMock = new Mock<IDbService>();
        _marketDataService = new MarketDataService(_marketDataClientMock.Object, _dbServiceMock.Object);
    }

    [Fact]
    public async Task RetrieveAndSaveMarketData_CallsUpdateLiveDistinctMarketData()
    {
        // Arrange
        var marketData = new List<MarketDataDto> { new MarketDataDto { Symbol = "AAPL" } };
        _marketDataClientMock.Setup(client => client.MarketdataAsync()).ReturnsAsync(marketData);

        // Act
        await _marketDataService.RetrieveAndSaveMarketData();

        // Assert
        _dbServiceMock.Verify(db => db.UpdateLiveDistinctMarketData(marketData), Times.Once);
    }

    [Fact]
    public async Task RetrieveAndSaveAvailableCompanies_CallsInitializeCompanies()
    {
        // Arrange
        var stockSymbols = new List<StockSymbolDto> { new StockSymbolDto { DisplaySymbol = "AAPL" } };
        _marketDataClientMock.Setup(client => client.StocksymbolsAsync()).ReturnsAsync(stockSymbols);

        // Act
        await _marketDataService.RetrieveAndSaveAvailableCompanies();

        // Assert
        _dbServiceMock.Verify(db => db.InitializeCompanies(stockSymbols), Times.Once);
    }

    [Fact]
    public async Task RefreshMarketData_CallsRetrieveAndSaveMarketData_WhenMarketIsOpen()
    {
        // Arrange
        var marketStatus = new MarketStatusDto { IsOpen = true };
        _marketDataClientMock.Setup(client => client.MarketstatusAsync()).ReturnsAsync(marketStatus);

        // Act
        await _marketDataService.RefreshMarketData();

        // Assert
        _marketDataClientMock.Verify(client => client.MarketdataAsync(), Times.Once);
        _dbServiceMock.Verify(db => db.UpdateLiveDistinctMarketData(It.IsAny<List<MarketDataDto>>()), Times.Never());
    }

    // [Fact]
    // public async Task RefreshMarketData_CallsUpdateTablesForMarketClosure_WhenMarketIsClosed()
    // {
    //     // Arrange
    //     var marketStatus = new MarketStatusDto { IsOpen = false };
    //     _marketDataClientMock.Setup(client => client.MarketstatusAsync()).ReturnsAsync(marketStatus);

    //     // Act
    //     await _marketDataService.RefreshMarketData();

    //     // Assert
    //     _marketDataClientMock.Verify(client => client.MarketdataAsync(), Times.Once);
    //     _dbServiceMock.Verify(db => db.UpdateLiveDistinctMarketData(It.IsAny<List<MarketDataDto>>()), Times.Never());
    //     _dbServiceMock.Verify(db => db.UpdatePriceHistory(), Times.Once);
    // }
}