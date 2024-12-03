using BusinessLogicDomain.API.Entities;
using BusinessLogicDomain.API.Models;

namespace BusinessLogicDomain.Tests.Unit.Fixtures
{
    public static class UnitTestFixture
    {
        public static UserRegisterDTO UserRegisterDTOMock => new()
        {
            UserName = "testUser",
            Password = "testPassword",
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.Now.AddYears(-30),
            Email = "testuser@example.com",
            Balance = 1000,
            SimulationLevel = BusinessLogicDomain.API.Entities.Enum.SimulationLevel.Easy
        };

        public static User UserMock => new()
        {
            ID = 1,
            UserName = "testUser",
            Password = "testPassword",
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.Now.AddYears(-30),
            Email = "testuser@example.com"
        };

        public static UserLoginDTO UserLoginDTOMock => new()
        {
            UserName = "testUser",
            Password = "testPassword"
        };

        public static UserInfoDTO UserInfoDTOMock => new()
        {
            FirstName = "Test",
            LastName = "User",
            DateOfBirth = DateTime.Now.AddYears(-30),
            Email = "testuser@example.com"
        };

        public static UserProfile UserProfileMock => new()
        {
            User = UserMock,
            Balance = 1000,
            SimulationLevel = BusinessLogicDomain.API.Entities.Enum.SimulationLevel.Easy,
            UserTransactions = new List<UserTransaction>(),
            UserPortfolioStocks = new List<PortfolioStock>()
        };
    }
}