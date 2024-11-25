using Moq;
using Xunit;
using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Entities.Enum;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class TransactionServiceTests
{
    private readonly Mock<IMarketDataDomainClient> _marketDataClientMock;
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _marketDataClientMock = new Mock<IMarketDataDomainClient>();
        _dbServiceMock = new Mock<IDbService>();
        _transactionService = new TransactionService(_marketDataClientMock.Object, _dbServiceMock.Object);
    }

    [Fact]
    public async Task ExecuteTransaction_CancelsTransaction_WhenInvalid()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            User = new User { ID = 1, UserName = "testuser", Password = "password", FirstName = "First", LastName = "Last", DateOfBirth = new DateTime(1990, 1, 1), Email = "test@example.com" },
            Balance = 1000m,
            SimulationLevel = SimulationLevel.Easy,
            UserTransactions = new List<UserTransaction>(),
            UserPortfolioStocks = new List<PortfolioStock>()
        };
        var transaction = new UserTransaction
        {
            ID = 1,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = TransactionType.Buy,
            TransactionValue = 1500m, // Invalid because it exceeds the balance
            StockValue = 50m,
            Quantity = 30,
            Company = new Company { ID = "AAPL", Name = "Apple Inc." },
            TimeOfTransaction = DateTime.Now
        };

        // Act
        var result = await _transactionService.ExecuteTransaction(userProfile, transaction);

        // Assert
        Assert.Equal(TransactionStatus.Cancelled, result.TransactionStatus);
        _dbServiceMock.Verify(db => db.UpdateTransaction(transaction), Times.Once);
    }

    [Fact]
    public async Task ExecuteTransaction_PutsTransactionOnHold_WhenMarketIsClosed()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            User = new User { ID = 1, UserName = "testuser", Password = "password", FirstName = "First", LastName = "Last", DateOfBirth = new DateTime(1990, 1, 1), Email = "test@example.com" },
            Balance = 1000m,
            SimulationLevel = SimulationLevel.Easy,
            UserTransactions = new List<UserTransaction>(),
            UserPortfolioStocks = new List<PortfolioStock>()
        };
        var transaction = new UserTransaction
        {
            ID = 1,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = TransactionType.Buy,
            TransactionValue = 500m,
            StockValue = 50m,
            Quantity = 10,
            Company = new Company { ID = "AAPL", Name = "Apple Inc." },
            TimeOfTransaction = DateTime.Now
        };
        _marketDataClientMock.Setup(client => client.MarketstatusAsync()).ReturnsAsync(new MarketStatusDto { IsOpen = false });

        // Act
        var result = await _transactionService.ExecuteTransaction(userProfile, transaction);

        // Assert
        Assert.Equal(TransactionStatus.OnHold, result.TransactionStatus);
        _dbServiceMock.Verify(db => db.UpdateTransaction(transaction), Times.Once);
    }

    [Fact]
    public async Task ExecuteTransaction_CompletesTransaction_WhenMarketIsOpen()
    {
        // Arrange
        var userProfile = new UserProfile
        {
            User = new User { ID = 1, UserName = "testuser", Password = "password", FirstName = "First", LastName = "Last", DateOfBirth = new DateTime(1990, 1, 1), Email = "test@example.com" },
            Balance = 1000m,
            SimulationLevel = SimulationLevel.Easy,
            UserTransactions = new List<UserTransaction>(),
            UserPortfolioStocks = new List<PortfolioStock>()
        };
        var transaction = new UserTransaction
        {
            ID = 1,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = TransactionType.Buy,
            TransactionValue = 500m,
            StockValue = 50m,
            Quantity = 10,
            Company = new Company { ID = "AAPL", Name = "Apple Inc." },
            TimeOfTransaction = DateTime.Now
        };
        _marketDataClientMock.Setup(client => client.MarketstatusAsync()).ReturnsAsync(new MarketStatusDto { IsOpen = true });

        // Act
        var result = await _transactionService.ExecuteTransaction(userProfile, transaction);

        // Assert
        Assert.Equal(TransactionStatus.Completed, result.TransactionStatus);
        _dbServiceMock.Verify(db => db.UpdateTransaction(transaction), Times.Once);
        _dbServiceMock.Verify(db => db.UpdateUserProfile(userProfile), Times.Once);
    }

    [Fact]
    public async Task CancelTransaction_CancelsOnHoldTransaction()
    {
        // Arrange
        var transaction = new UserTransaction
        {
            ID = 1,
            TransactionStatus = TransactionStatus.OnHold,
            TransactionType = TransactionType.Buy,
            TransactionValue = 500m,
            StockValue = 50m,
            Quantity = 10,
            Company = new Company { ID = "AAPL", Name = "Apple Inc." },
            TimeOfTransaction = DateTime.Now
        };

        // Act
        var result = await _transactionService.CancelTransaction(transaction);

        // Assert
        Assert.Equal(TransactionStatus.Cancelled, result.TransactionStatus);
        _dbServiceMock.Verify(db => db.UpdateTransaction(transaction), Times.Once);
    }

    [Fact]
    public async Task CreateIndividualJobs_RefreshesUserProfiles()
    {
        // Arrange
        var userProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                User = new User { ID = 1, UserName = "testuser", Password = "password", FirstName = "First", LastName = "Last", DateOfBirth = new DateTime(1990, 1, 1), Email = "test@example.com" },
                Balance = 1000m,
                SimulationLevel = SimulationLevel.Easy,
                UserTransactions = new List<UserTransaction>
                {
                    new UserTransaction
                    {
                        ID = 1,
                        TransactionStatus = TransactionStatus.OnHold,
                        TransactionType = TransactionType.Buy,
                        TransactionValue = 500m,
                        StockValue = 50m,
                        Quantity = 10,
                        Company = new Company { ID = "AAPL", Name = "Apple Inc." },
                        TimeOfTransaction = DateTime.Now
                    }
                },
                UserPortfolioStocks = new List<PortfolioStock>()
            }
        };
        _dbServiceMock.Setup(db => db.RetrieveAllUserProfiles()).ReturnsAsync(userProfiles);

        _marketDataClientMock.Setup(client => client.MarketstatusAsync()).ReturnsAsync(new MarketStatusDto { IsOpen = true });

        // Act
        await _transactionService.CreateIndividualJobs();

        // Assert
        _dbServiceMock.Verify(db => db.UpdateUserProfile(It.IsAny<UserProfile>()), Times.AtLeastOnce);
    }
}