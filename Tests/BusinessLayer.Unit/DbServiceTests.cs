using Moq;
using AutoMapper;
using BusinessLogicDomain.API.Context.YouTradeDbContext;
using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Models;
using Microsoft.EntityFrameworkCore;
using BusinessLogicDomain.API.Entities.Enum;
using BusinessLogicDomain.MarketDataDomainAPIClient;

public class DbServiceTests : IClassFixture<UnitTestFixture>
{
    private readonly UnitTestFixture _fixture;
    private readonly Mock<IMapper> _mapperMock;
    private readonly DbService _dbService;
    private readonly YouTradeContext _context;

    public DbServiceTests(UnitTestFixture fixture)
    {
        _fixture = fixture;

        var options = new DbContextOptionsBuilder<YouTradeContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new YouTradeContext(options);
        _mapperMock = new Mock<IMapper>();
        _dbService = new DbService(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task InitializeCompanies_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var stockSymbols = new List<StockSymbolDto>
        {
            new StockSymbolDto
            {
                DisplaySymbol = "AAPL",
                Currency = "USD",
                Description = "Apple Inc.",
                Figi = "BBG000B9XRY4",
                Mic = "XNAS",
                Symbol = "AAPL",
                Type = "Common Stock"
            }
        };

        _mapperMock.Setup(mapper => mapper.Map<Company>(It.IsAny<StockSymbolDto>()))
                   .Returns(new Company { ID = "AAPL", Name = "Apple Inc." });

        // Act
        await _dbService.InitializeCompanies(stockSymbols);

        // Assert
        var companies = await _context.Companies.ToListAsync();
        Assert.Single(companies);
    }

    [Fact]
    public async Task UpdateLiveDistinctMarketData_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var marketData = new List<MarketDataDto>
        {
            new MarketDataDto
            {
            Symbol = "AAPL",
            Description = "Apple Inc.",
            Currency = "USD",
            CurrentPrice = 150.0,
            HighPrice = 155.0,
            LowPrice = 145.0,
            OpenPrice = 148.0,
            PreviousClosePrice = 149.0,
            Change = 1.0,
            PercentChange = 0.67,
            Date = DateTime.Now
            }
        };

        _mapperMock.Setup(mapper => mapper.Map<LivePriceDistinct>(It.IsAny<MarketDataDto>()))
                   .Returns(new LivePriceDistinct { ID = "AAPL", Price = 150m, Date = DateTime.Now });

        // Act
        await _dbService.UpdateLiveDistinctMarketData(marketData);

        // Assert
        var livePriceDistincts = await _context.LivePriceDistinct.ToListAsync();
        Assert.Single(livePriceDistincts);
    }

    [Fact]
    public async Task UpdatePriceHistory_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var company = new Company { ID = "AAPL", Name = "Apple Inc." };

        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        var livePriceDaily = new LivePriceDaily { ID = 1, Price = 150m, Date = DateTime.Now, Company = company };
        await _context.LivePriceDaily.AddAsync(livePriceDaily);
        await _context.SaveChangesAsync();

        _mapperMock.Setup(mapper => mapper.Map<PriceHistory>(It.IsAny<LivePriceDaily>()))
                   .Returns(new PriceHistory { ID = 1, EODPrice = 150m, Date = DateTime.Now, Company = company });

        // Act
        await _dbService.UpdatePriceHistory();

        // Assert
        var priceHistories = await _context.PriceHistories.ToListAsync();
        Assert.NotEmpty(priceHistories);
    }

    [Fact]
    public async Task RetrieveInitializedCompanies_ReturnsCompanies()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var companies = new List<Company> { new Company { ID = "AAPL", Name = "Apple Inc." } };
        await _context.Companies.AddRangeAsync(companies);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveInitializedCompanies();

        // Assert
        Assert.Equal(companies, result);
    }

    [Fact]
    public async Task RetrieveCompanyBySymbol_ReturnsCompany()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var company = new Company { ID = "AAPL", Name = "Apple Inc." };
        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveCompanyBySymbol("AAPL");

        // Assert
        Assert.Equal(company, result);
    }

    [Fact]
    public async Task GetCompanyLivePriceDistinct_ReturnsLivePriceDistinct()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var livePriceDistinct = new LivePriceDistinct { ID = "AAPL", Price = 0m, Date = DateTime.Now };
        await _context.LivePriceDistinct.AddAsync(livePriceDistinct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.GetCompanyLivePriceDistinct("AAPL");

        // Assert
        Assert.Equal(livePriceDistinct, result);
    }

    [Fact]
    public async Task GetCompanyLivePriceDaily_ReturnsLivePriceDaily()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var livePriceDaily = new List<LivePriceDaily> { new LivePriceDaily { ID = 1, Price = 0m, Date = DateTime.Now, Company = new Company { ID = "AAPL", Name = "Apple Inc." } } };
        await _context.LivePriceDaily.AddRangeAsync(livePriceDaily);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.GetCompanyLivePriceDaily("AAPL");

        // Assert
        Assert.Equal(livePriceDaily, result);
    }

    [Fact]
    public async Task GetCompanyPriceHistory_ReturnsPriceHistory()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        var date = new DateTime(2021, 1, 1);

        // Arrange
        var priceHistory = new List<PriceHistory>
        {
            new PriceHistory
            {
                ID = 1,
                EODPrice = 150m,
                Date = date,
                Company = new Company { ID = "AAPL", Name = "Apple Inc." }
            }
        };
        await _context.PriceHistories.AddRangeAsync(priceHistory);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.GetCompanyPriceHistory("AAPL", new DateTime(2020, 1, 1), DateTime.Now);

        // Assert
        Assert.Equal(priceHistory, result);
    }

    [Fact]
    public async Task CreateUserProfile_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var user = new User
        {
            ID = 1,
            UserName = "testuser",
            Password = "password",
            FirstName = "First",
            LastName = "Last",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "test@example.com"
        };
        var balance = 1000m;
        var simulationLevel = SimulationLevel.Easy;

        // Act
        await _dbService.CreateUserProfile(user, balance, simulationLevel);

        // Assert
        var userProfiles = await _context.UserProfiles.ToListAsync();
        Assert.Single(userProfiles);
    }

    [Fact]
    public async Task CreateUser_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var newUser = new UserRegisterDTO
        {
            UserName = "testuser",
            Password = "password",
            FirstName = "First",
            LastName = "Last",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "test@example.com",
            Balance = 1000m,
            SimulationLevel = SimulationLevel.Easy
        };

        // Act
        await _dbService.CreateUser(newUser);

        // Assert
        var users = await _context.Users.ToListAsync();
        Assert.Single(users);
    }

    [Fact]
    public async Task RetrieveUser_ReturnsUser()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var user = new User
        {
            ID = 1,
            UserName = "testuser",
            Password = "password",
            FirstName = "First",
            LastName = "Last",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "test@example.com"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveUser(1);

        // Assert
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task RetrieveUserByUsername_ReturnsUser()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var user = new User
        {
            ID = 1,
            UserName = "testuser",
            Password = "password",
            FirstName = "First",
            LastName = "Last",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "test@example.com"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveUserByUsername("testuser");

        // Assert
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task RetrieveUserByEmail_ReturnsUser()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var user = new User
        {
            ID = 1,
            UserName = "testuser",
            Password = "password",
            FirstName = "First",
            LastName = "Last",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "test@example.com"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveUserByEmail("test@example.com");

        // Assert
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task RetrieveUserProfile_ReturnsUserProfile()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var userProfile = new UserProfile
        {
            User = new User
            {
                ID = 1,
                UserName = "testuser",
                Password = "password",
                FirstName = "First",
                LastName = "Last",
                DateOfBirth = new DateTime(1990, 1, 1),
                Email = "test@example.com"
            },
            Balance = 1000m,
            SimulationLevel = SimulationLevel.Easy,
            UserTransactions = new List<UserTransaction>(),
            UserPortfolioStocks = new List<PortfolioStock>()
        };
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveUserProfile(1);

        // Assert
        Assert.Equal(userProfile, result);
    }

    [Fact]
    public async Task UpdateUserProfile_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var userProfile = new UserProfile
        {
            User = new User
            {
                ID = 1,
                UserName = "testuser",
                Password = "password",
                FirstName = "First",
                LastName = "Last",
                DateOfBirth = new DateTime(1990, 1, 1),
                Email = "test@example.com"
            },
            Balance = 1000m,
            SimulationLevel = SimulationLevel.Easy,
            UserTransactions = new List<UserTransaction>(),
            UserPortfolioStocks = new List<PortfolioStock>()
        };
        await _context.UserProfiles.AddAsync(userProfile);
        await _context.SaveChangesAsync();

        // Act
        await _dbService.UpdateUserProfile(userProfile);

        // Assert
        var updatedUserProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.User.ID == 1);
        Assert.Equal(userProfile, updatedUserProfile);
    }

    [Fact]
    public async Task UpdateUser_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var user = new User
        {
            ID = 1,
            UserName = "testuser",
            Password = "password",
            FirstName = "First",
            LastName = "Last",
            DateOfBirth = new DateTime(1990, 1, 1),
            Email = "test@example.com"
        };
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        await _dbService.UpdateUser(user);

        // Assert
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.ID == 1);
        Assert.Equal(user, updatedUser);
    }

    [Fact]
    public async Task GetCurrentStockPriceOfCompany_ReturnsPrice()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var livePriceDistinct = new LivePriceDistinct { ID = "AAPL", Price = 150m, Date = DateTime.Now };
        await _context.LivePriceDistinct.AddAsync(livePriceDistinct);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.GetCurrentStockPriceOfCompany("AAPL");

        // Assert
        Assert.Equal(150m, result);
    }

    [Fact]
    public async Task UpdateTransaction_CallsSaveChangesAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var transaction = new UserTransaction
        {
            ID = 1,
            TransactionType = TransactionType.Buy,
            TransactionStatus = TransactionStatus.Pending,
            Company = new Company { ID = "AAPL", Name = "Apple Inc." },
            TransactionValue = 1000m,
            StockValue = 150m,
            Quantity = 10,
            TimeOfTransaction = DateTime.Now
        };
        await _context.UserTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        await _dbService.UpdateTransaction(transaction);

        // Assert
        var updatedTransaction = await _context.UserTransactions.FirstOrDefaultAsync(t => t.ID == 1);
        Assert.Equal(transaction, updatedTransaction);
    }

    [Fact]
    public async Task RetrieveAllUserProfiles_ReturnsUserProfiles()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var userProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                User = new User
                {
                    ID = 1,
                    UserName = "testuser",
                    Password = "password",
                    FirstName = "First",
                    LastName = "Last",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Email = "test@example.com"
                },
                Balance = 1000m,
                SimulationLevel = SimulationLevel.Easy,
                UserTransactions = new List<UserTransaction>(),
                UserPortfolioStocks = new List<PortfolioStock>()
            }
        };
        await _context.UserProfiles.AddRangeAsync(userProfiles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveAllUserProfiles();

        // Assert
        Assert.Equal(userProfiles, result);
    }

    [Fact]
    public async Task RetrieveUserTransaction_ReturnsTransaction()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Arrange
        var transaction = new UserTransaction
        {
            ID = 1,
            TransactionType = TransactionType.Buy,
            TransactionStatus = TransactionStatus.Pending,
            Company = new Company { ID = "AAPL", Name = "Apple Inc." },
            TransactionValue = 1000m,
            StockValue = 150m,
            Quantity = 10,
            TimeOfTransaction = DateTime.Now
        };
        await _context.UserTransactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _dbService.RetrieveUserTransaction(1);

        // Assert
        Assert.Equal(transaction, result);
    }
}