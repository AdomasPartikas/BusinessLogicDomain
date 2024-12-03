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
        var marketData = new List<MarketDataDto> { new() { Symbol = "AAPL" } };
        _marketDataClientMock.Setup(client => client.MarketdataAsync()).ReturnsAsync(marketData);

        // Act
        await _marketDataService.RetrieveAndSaveMarketData();

        // Assert
        _dbServiceMock.Verify(db => db.UpdateLiveDistinctMarketData(marketData), Times.Once);
    }

    [Fact]
    public async Task RetrieveAndSaveMarketData_DoesNotCallUpdateLiveDistinctMarketData_WhenMarketDataIsNull()
    {
        // Arrange
        _marketDataClientMock.Setup(client => client.MarketdataAsync()).ReturnsAsync((List<MarketDataDto>?)null);

        // Act
        await _marketDataService.RetrieveAndSaveMarketData();

        // Assert
        _dbServiceMock.Verify(db => db.UpdateLiveDistinctMarketData(It.IsAny<List<MarketDataDto>>()), Times.Never);
    }

    [Fact]
    public async Task RetrieveAndSaveAvailableCompanies_DoesNotCallInitializeCompanies_WhenStockSymbolsAreNull()
    {
        // Arrange
        _marketDataClientMock.Setup(client => client.StocksymbolsAsync()).ReturnsAsync((List<StockSymbolDto>?)null);

        // Act
        await _marketDataService.RetrieveAndSaveAvailableCompanies();

        // Assert
        _dbServiceMock.Verify(db => db.InitializeCompanies(It.IsAny<List<StockSymbolDto>>()), Times.Never);
    }

    [Fact]
    public async Task RetrieveAndSaveAvailableCompanies_CallsInitializeCompanies()
    {
        // Arrange
        var stockSymbols = new List<StockSymbolDto> { new() { DisplaySymbol = "AAPL" } };
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

    [Fact]
    public async Task RefreshMarketData_DoesNotCallAnyMethods_WhenMarketStatusIsNull()
    {
        // Arrange
        _marketDataClientMock.Setup(client => client.MarketstatusAsync()).ReturnsAsync((MarketStatusDto?)null);

        // Act
        await _marketDataService.RefreshMarketData();

        // Assert
        _marketDataClientMock.Verify(client => client.MarketdataAsync(), Times.Never);
        _dbServiceMock.Verify(db => db.UpdateLiveDistinctMarketData(It.IsAny<List<MarketDataDto>>()), Times.Never);
        _dbServiceMock.Verify(db => db.UpdatePriceHistory(), Times.Never);
    }

    [Fact]
    public async Task RefreshMarketData_CallsUpdatePriceHistory_WhenMarketIsClosed()
    {
        // Arrange
        var marketStatus = new MarketStatusDto { IsOpen = false };
        _marketDataClientMock.Setup(client => client.MarketstatusAsync()).ReturnsAsync(marketStatus);

        // Act
        await _marketDataService.RefreshMarketData();

        // Assert
        _dbServiceMock.Verify(db => db.UpdatePriceHistory(), Times.Once);
        _marketDataClientMock.Verify(client => client.MarketdataAsync(), Times.Never);
    }
}