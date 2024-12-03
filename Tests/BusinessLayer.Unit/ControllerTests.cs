using Xunit;
using BusinessLogicDomain.API.Controller;
using BusinessLogicDomain.API.Services;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BusinessLogicDomain.API.Models;
using BusinessLogicDomain.Tests.Unit.Fixtures;

namespace BusinessLogicDomain.Tests.Unit;


public class MarketDataControllerTests
{
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly MarketDataController _controller;

    public MarketDataControllerTests()
    {
        _dbServiceMock = new Mock<IDbService>();
        _controller = new MarketDataController(_dbServiceMock.Object);
    }

    [Fact]
    public async Task GetAllCompanies_ReturnsOkResult()
    {
        // Arrange
        var companies = new List<BusinessLogicDomain.API.Entities.Company>
            {
                new() { ID = "COO1", Name = "Company1" },
                new() { ID = "COO2", Name = "Company2" }
            };
        _dbServiceMock.Setup(service => service.RetrieveInitializedCompanies())
                      .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAllCompanies();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnCompanies = Assert.IsType<List<BusinessLogicDomain.API.Entities.Company>>(okResult.Value);
        Assert.Equal(companies.Count, returnCompanies.Count);
    }

    [Fact]
    public async Task GetAllCompanies_ReturnsNoContent()
    {
        // Arrange
        _dbServiceMock.Setup(service => service.RetrieveInitializedCompanies())!
                      .ReturnsAsync((List<API.Entities.Company>?)null);

        // Act
        var result = await _controller.GetAllCompanies();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetAllCompanies_ReturnsEmptyList()
    {
        // Arrange
        var companies = new List<API.Entities.Company>();
        _dbServiceMock.Setup(service => service.RetrieveInitializedCompanies())
                      .ReturnsAsync(companies);

        // Act
        var result = await _controller.GetAllCompanies();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnCompanies = Assert.IsType<List<API.Entities.Company>>(okResult.Value);
        Assert.Empty(returnCompanies);
    }

    [Fact]
    public async Task GetCompanyLivePriceDistinct_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetCompanyLivePriceDistinct(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetCompanyLivePriceDistinct_ReturnsNoContent()
    {
        // Arrange
        var symbols = "AAPL,GOOGL";
        _dbServiceMock.Setup(service => service.RetrieveCompanyBySymbol(It.IsAny<string>()))!
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.Company?)null);

        // Act
        var result = await _controller.GetCompanyLivePriceDistinct(symbols);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetCompanyLivePriceDistinct_ReturnsOkResult()
    {
        // Arrange
        var symbols = "AAPL,GOOGL";
        var company = new BusinessLogicDomain.API.Entities.Company { ID = "1", Name = "Company1" };
        var prices = new List<BusinessLogicDomain.API.Entities.LivePriceDistinct>
        {
            new() { ID = "AAPL", Price = 150.00M, Date = DateTime.Now },
            new() { ID = "GOOGL", Price = 2800.00M, Date = DateTime.Now }
        };

        _dbServiceMock.Setup(service => service.RetrieveCompanyBySymbol(It.IsAny<string>()))
                      .ReturnsAsync(company);
        _dbServiceMock.Setup(service => service.GetCompanyLivePriceDistinct(It.IsAny<string>()))
                      .ReturnsAsync((string symbol) => prices.FirstOrDefault(p => p.ID == symbol)!);

        // Act
        var result = await _controller.GetCompanyLivePriceDistinct(symbols);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnPrices = Assert.IsType<List<BusinessLogicDomain.API.Entities.LivePriceDistinct>>(okResult.Value);
        Assert.Equal(prices.Count, returnPrices.Count);
    }

    [Fact]
    public async Task GetCompanyLivePriceDaily_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetCompanyLivePriceDaily(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Symbol parameter is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCompanyLivePriceDaily_ReturnsNoContent()
    {
        // Arrange
        var symbol = "AAPL";
        _dbServiceMock.Setup(service => service.RetrieveCompanyBySymbol(symbol))!
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.Company?)null);

        // Act
        var result = await _controller.GetCompanyLivePriceDaily(symbol);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetCompanyLivePriceDaily_ReturnsOkResult()
    {
        // Arrange
        var symbol = "AAPL";
        var company = new BusinessLogicDomain.API.Entities.Company { ID = "1", Name = "Company1" };
        var dailyPrices = new List<BusinessLogicDomain.API.Entities.LivePriceDaily>
        {
            new() { ID = 1, Company = new API.Entities.Company{ID = "AAPL", Name = "Apple"}, Price = 150.00M, Date = DateTime.Now },
            new() { ID = 2, Company = new API.Entities.Company{ID = "AAPL", Name = "Apple"}, Price = 151.00M, Date = DateTime.Now.AddDays(-1) }
        };

        _dbServiceMock.Setup(service => service.RetrieveCompanyBySymbol(symbol))
                      .ReturnsAsync(company);
        _dbServiceMock.Setup(service => service.GetCompanyLivePriceDaily(symbol))
                      .ReturnsAsync(dailyPrices);

        // Act
        var result = await _controller.GetCompanyLivePriceDaily(symbol);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnPrices = Assert.IsType<List<BusinessLogicDomain.API.Entities.LivePriceDaily>>(okResult.Value);
        Assert.Equal(dailyPrices.Count, returnPrices.Count);
    }

    [Fact]
    public async Task GetCompanyPriceHistory_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetCompanyPriceHistory(null!, DateTime.Now.AddDays(-30), DateTime.Now);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Symbol parameter is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCompanyPriceHistory_ReturnsNoContent()
    {
        // Arrange
        var symbol = "AAPL";
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;
        _dbServiceMock.Setup(service => service.RetrieveCompanyBySymbol(symbol))!
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.Company?)null);

        // Act
        var result = await _controller.GetCompanyPriceHistory(symbol, startDate, endDate);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task GetCompanyPriceHistory_ReturnsOkResult()
    {
        // Arrange
        var symbol = "AAPL";
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;
        var company = new BusinessLogicDomain.API.Entities.Company { ID = "1", Name = "Company1" };
        var priceHistory = new List<BusinessLogicDomain.API.Entities.PriceHistory>
        {
            new() { ID = 1, Company = new API.Entities.Company{ID = "AAPL", Name = "Apple"}, EODPrice = 150.00M, Date = DateTime.Now.AddDays(-1) },
            new() { ID = 2, Company = new API.Entities.Company{ID = "AAPL", Name = "Apple"}, EODPrice = 151.00M, Date = DateTime.Now.AddDays(-2) }
        };

        _dbServiceMock.Setup(service => service.RetrieveCompanyBySymbol(symbol))
                      .ReturnsAsync(company);
        _dbServiceMock.Setup(service => service.GetCompanyPriceHistory(symbol, startDate, endDate))
                      .ReturnsAsync(priceHistory);

        // Act
        var result = await _controller.GetCompanyPriceHistory(symbol, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnPrices = Assert.IsType<List<BusinessLogicDomain.API.Entities.PriceHistory>>(okResult.Value);
        Assert.Equal(priceHistory.Count, returnPrices.Count);
    }
}

public class UserControllerTests
{
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _dbServiceMock = new Mock<IDbService>();
        _controller = new UserController(_dbServiceMock.Object);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_UserAlreadyExists()
    {
        // Arrange
        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(UnitTestFixture.UserRegisterDTOMock.UserName))
                      .ReturnsAsync(UnitTestFixture.UserMock);

        // Act
        var result = await _controller.Register(UnitTestFixture.UserRegisterDTOMock);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_EmailAlreadyExists()
    {
        // Arrange
        var newUser = UnitTestFixture.UserRegisterDTOMock;
        newUser.UserName = "newUser";

        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(newUser.UserName))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);
        _dbServiceMock.Setup(service => service.RetrieveUserByEmail(newUser.Email))
                      .ReturnsAsync(UnitTestFixture.UserMock);

        // Act
        var result = await _controller.Register(newUser);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email already exists", badRequestResult.Value);
    }

    [Fact]
public async Task Register_ReturnsOkResult()
    {
        // Arrange
        var newUser = UnitTestFixture.UserRegisterDTOMock;
        var createdUser = new BusinessLogicDomain.API.Entities.User
        {
            ID = 1,
            UserName = newUser.UserName,
            Password = newUser.Password,
            Email = newUser.Email,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            DateOfBirth = newUser.DateOfBirth
        };

        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(newUser.UserName))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);
        _dbServiceMock.Setup(service => service.RetrieveUserByEmail(newUser.Email))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);
        _dbServiceMock.Setup(service => service.CreateUser(newUser))
                      .ReturnsAsync(createdUser);

        // Act
        var result = await _controller.Register(newUser);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUser = Assert.IsType<BusinessLogicDomain.API.Entities.User>(okResult.Value);
        Assert.Equal(createdUser.ID, returnUser.ID);
        Assert.Equal(createdUser.UserName, returnUser.UserName);
        Assert.Equal(createdUser.Email, returnUser.Email);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_UserDoesNotExist()
    {
        // Arrange
        var userLogin = UnitTestFixture.UserLoginDTOMock;

        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(userLogin.UserName))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);

        // Act
        var result = await _controller.Login(userLogin);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User does not exist", badRequestResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WhenPasswordIsIncorrect()
    {
        // Arrange
        var userLogin = UnitTestFixture.UserLoginDTOMock;
        var existingUser = UnitTestFixture.UserMock;
        existingUser.Password = "correctpassword";

        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(userLogin.UserName))
                      .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Login(userLogin);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Incorrect password", badRequestResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsOkResult()
    {
        // Arrange
        var userLogin = UnitTestFixture.UserLoginDTOMock;
        var existingUser = UnitTestFixture.UserMock;

        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(userLogin.UserName))
                      .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Login(userLogin);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUser = Assert.IsType<BusinessLogicDomain.API.Entities.User>(okResult.Value);
        Assert.Equal(existingUser.ID, returnUser.ID);
        Assert.Equal(existingUser.UserName, returnUser.UserName);
    }

    [Fact]
    public async Task GetUser_ReturnsBadRequest_UserDoesNotExist()
    {
        // Arrange
        var userId = 1;
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User does not exist", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUser_ReturnsOkResult_UserIsFound()
    {
        // Arrange
        var userId = 1;
        var existingUser = UnitTestFixture.UserMock;
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUser = Assert.IsType<BusinessLogicDomain.API.Entities.User>(okResult.Value);
        Assert.Equal(existingUser.ID, returnUser.ID);
        Assert.Equal(existingUser.UserName, returnUser.UserName);
    }

    [Fact]
    public async Task UpdateUsername_ReturnsBadRequest_UserDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var newUsername = "newUsername";
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);

        // Act
        var result = await _controller.UpdateUsername(userId, newUsername);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User does not exist", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUsername_ReturnsBadRequest_UsernameAlreadyExists()
    {
        // Arrange
        var userId = 1;
        var newUsername = "newUsername";
        var existingUserWithUsername = UnitTestFixture.UserMock;
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync(UnitTestFixture.UserMock);
        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(newUsername))
                      .ReturnsAsync(existingUserWithUsername);

        // Act
        var result = await _controller.UpdateUsername(userId, newUsername);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Username already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUsername_ReturnsOkResult_UsernameIsUpdatedSuccessfully()
    {
        // Arrange
        var userId = 1;
        var newUsername = "newUsername";
        var existingUser = UnitTestFixture.UserMock;
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync(existingUser);
        _dbServiceMock.Setup(service => service.RetrieveUserByUsername(newUsername))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);
        _dbServiceMock.Setup(service => service.UpdateUser(existingUser))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateUsername(userId, newUsername);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUser = Assert.IsType<BusinessLogicDomain.API.Entities.User>(okResult.Value);
        Assert.Equal(newUsername, returnUser.UserName);
    }

    [Fact]
    public async Task UpdatePassword_ReturnsBadRequest_UserDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var newPassword = "newPassword";
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);

        // Act
        var result = await _controller.UpdatePassword(userId, newPassword);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User does not exist", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdatePassword_ReturnsOkResult_PasswordIsUpdatedSuccessfully()
    {
        // Arrange
        var userId = 1;
        var newPassword = "newPassword";
        var existingUser = UnitTestFixture.UserMock;
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync(existingUser);
        _dbServiceMock.Setup(service => service.UpdateUser(existingUser))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdatePassword(userId, newPassword);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUser = Assert.IsType<BusinessLogicDomain.API.Entities.User>(okResult.Value);
        Assert.Equal(newPassword, returnUser.Password);
    }

    [Fact]
    public async Task UpdateUserInfo_ReturnsBadRequest_UserDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var userInfo = UnitTestFixture.UserInfoDTOMock;
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);

        // Act
        var result = await _controller.UpdateUserInfo(userId, userInfo);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User does not exist", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUserInfo_ReturnsBadRequest_EmailAlreadyExists()
    {
        // Arrange
        var userId = 1;
        var userInfo = UnitTestFixture.UserInfoDTOMock;
        userInfo.Email = "test";

        var existingUserWithEmail = UnitTestFixture.UserMock;
        
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync(UnitTestFixture.UserMock);
        _dbServiceMock.Setup(service => service.RetrieveUserByEmail(userInfo.Email))
                      .ReturnsAsync(existingUserWithEmail);

        // Act
        var result = await _controller.UpdateUserInfo(userId, userInfo);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUserInfo_ReturnsOkResult_WhenUserInfoIsUpdatedSuccessfully()
    {
        // Arrange
        var userId = 1;
        var userInfo = UnitTestFixture.UserInfoDTOMock;
        var existingUser = UnitTestFixture.UserMock;
        _dbServiceMock.Setup(service => service.RetrieveUser(userId))
                      .ReturnsAsync(existingUser);
        _dbServiceMock.Setup(service => service.RetrieveUserByEmail(userInfo.Email))
                      .ReturnsAsync((BusinessLogicDomain.API.Entities.User?)null);
        _dbServiceMock.Setup(service => service.UpdateUser(existingUser))
                      .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateUserInfo(userId, userInfo);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnUser = Assert.IsType<BusinessLogicDomain.API.Entities.User>(okResult.Value);
        Assert.Equal(userInfo.Email, returnUser.Email);
        Assert.Equal(userInfo.FirstName, returnUser.FirstName);
        Assert.Equal(userInfo.LastName, returnUser.LastName);
    }
}

public class UserProfileControllerTests
{
    private readonly Mock<IDbService> _dbServiceMock;
    private readonly Mock<ITransactionService> _transactionServiceMock;
    private readonly UserProfileController _controller;

    public UserProfileControllerTests()
    {
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