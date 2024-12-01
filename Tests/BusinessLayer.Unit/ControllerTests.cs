using Xunit;
using BusinessLogicDomain.API.Controller;
using BusinessLogicDomain.API.Services;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BusinessLogicDomain.Tests.Unit;


public class MarketDataControllerTests : IClassFixture<UnitTestFixture>
{
    private readonly UnitTestFixture _fixture;
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly MarketDataController _controller;

    public MarketDataControllerTests(UnitTestFixture fixture)
    {
        _fixture = fixture;
        _dbServiceMock = new Mock<IDbService>();
        _controller = new MarketDataController(_dbServiceMock.Object);
    }

    [Fact]
    public async Task GetAllCompanies_ReturnsOkResult()
    {
        // Arrange
        _dbServiceMock.Setup(service => service.RetrieveInitializedCompanies())
                      .ReturnsAsync(new List<BusinessLogicDomain.API.Entities.Company>());

        // Act
        var result = await _controller.GetAllCompanies();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCompanyLivePriceDistinct_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetCompanyLivePriceDistinct(null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}

public class UserControllerTests : IClassFixture<UnitTestFixture>
{
    private readonly UnitTestFixture _fixture;
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly UserController _controller;

    public UserControllerTests(UnitTestFixture fixture)
    {
        _fixture = fixture;
        _dbServiceMock = new Mock<IDbService>();
        _controller = new UserController(_dbServiceMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenUserExists()
    {
        // Arrange
        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(It.IsAny<string>()))
                      .ReturnsAsync(new BusinessLogicDomain.API.Entities.User
                      {
                          UserName = "existingUser",
                          Password = "existingPassword",
                          FirstName = "Existing",
                          LastName = "User",
                          DateOfBirth = DateTime.Now.AddYears(-30),
                          Email = "existinguser@example.com"
                      });

        // Act
        var result = await _controller.Register(new BusinessLogicDomain.API.Models.UserRegisterDTO
        {
            UserName = "testUser",
            Password = "testPassword",
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.Now.AddYears(-30),
            Email = "testuser@example.com",
            Balance = 1000,
            SimulationLevel = BusinessLogicDomain.API.Entities.Enum.SimulationLevel.Easy
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Act
        var result = await _controller.Login(new BusinessLogicDomain.API.Models.UserLoginDTO
        {
            UserName = "testUser",
            Password = "testPassword"
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}

public class UserProfileControllerTests : IClassFixture<UnitTestFixture>
{
    private readonly UnitTestFixture _fixture;
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly UserProfileController _controller;

    public UserProfileControllerTests(UnitTestFixture fixture)
    {
        _fixture = fixture;
        _dbServiceMock = new Mock<IDbService>();
        _transactionServiceMock = new Mock<ITransactionService>();
        _controller = new UserProfileController(_dbServiceMock.Object, _transactionServiceMock.Object);
    }

    [Fact]
    public async Task GetUserProfile_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Act
        var result = await _controller.GetUserProfile(1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Buy_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        // Act
        var result = await _controller.Buy(1, new BusinessLogicDomain.API.Models.BuyStockDTO
        {
            Symbol = "AAPL",
            Value = 100
        });

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}