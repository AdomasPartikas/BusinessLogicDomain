using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Microsoft.AspNetCore.Mvc;

namespace BusinessLogicDomain.API.Controller
{
    [Route("api/marketdata")]
    [ApiController]
    public class MarketDataController(IDbService dbService) : ControllerBase
    {

        [HttpGet("getallcompanies")]
        [ProducesResponseType(200, Type = typeof(List<Entities.Company>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await dbService.RetrieveInitializedCompanies();

            if (companies == null)
                return NoContent();

            return Ok(companies);
        }

        [HttpGet("marketdata/getcompanylivepricedistinct")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Entities.LivePriceDistinct>))]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetCompanyLivePriceDistinct([FromQuery] string symbols)
        {
            if (string.IsNullOrEmpty(symbols))
                return BadRequest("Symbols parameter is required.");

            var symbolList = symbols.Split(',').Select(s => s.Trim()).ToList();

            var prices = new List<Entities.LivePriceDistinct>();

            foreach (var symbol in symbolList)
            {
                var company = await dbService.RetrieveCompanyBySymbol(symbol);
                if (company == null)
                    continue;

                var price = await dbService.GetCompanyLivePriceDistinct(symbol);
                if (price != null)
                    prices.Add(price);
            }

            if (prices.Count == 0)
                return NoContent();

            return Ok(prices);
        }

        [HttpGet("marketdata/getcompanylivepricedaily")]
        [ProducesResponseType(200, Type = typeof(List<Entities.LivePriceDaily>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCompanyLivePriceDaily([FromQuery] string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return BadRequest("Symbol parameter is required.");

            var company = await dbService.RetrieveCompanyBySymbol(symbol);

            if (company == null)
                return NoContent();

            var priceHistory = await dbService.GetCompanyLivePriceDaily(symbol);

            if (priceHistory == null)
                return NoContent();

            return Ok(priceHistory);
        }

        [HttpGet("marketdata/getcompanypricehistory")]
        [ProducesResponseType(200, Type = typeof(List<Entities.PriceHistory>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCompanyPriceHistory([FromQuery] string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (string.IsNullOrEmpty(symbol))
                return BadRequest("Symbol parameter is required.");

            var company = await dbService.RetrieveCompanyBySymbol(symbol);

            if (company == null)
                return NoContent();

            var priceHistory = await dbService.GetCompanyPriceHistory(symbol, startDate, endDate);

            if (priceHistory == null || priceHistory.Count == 0)
                return NoContent();

            return Ok(priceHistory);
        }
    }

    [Route("api/user")]
    [ApiController]
    public class UserController(IDbService dbService) : ControllerBase
    {

        private readonly IDbService _dbService = dbService;

        [HttpPost("register")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] Models.UserRegisterDto newUser)
        {
            var existingUser = await _dbService.RetrieveUserByUsername(newUser.UserName);

            if (existingUser != null)
                return BadRequest("User already exists");

            var userWithEmail = await _dbService.RetrieveUserByEmail(newUser.Email);

            if (userWithEmail != null)
                return BadRequest("Email already exists");

            var user = await _dbService.CreateUser(newUser);

            return Ok(user);
        }

        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login([FromBody] Models.UserLoginDto userLogin)
        {
            var existingUser = await _dbService.RetrieveUserByUsername(userLogin.UserName);

            if (existingUser == null)
                return BadRequest("User does not exist");

            if (existingUser.Password != userLogin.Password)
                return BadRequest("Incorrect password");

            return Ok(existingUser);
        }

        [HttpPost("getuser")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUser([FromQuery] int id)
        {
            var user = await _dbService.RetrieveUser(id);

            if (user == null)
                return BadRequest("User does not exist");

            return Ok(user);
        }

        [HttpPost("updateusername")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateUsername([FromQuery] int id, [FromQuery] string username)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
                return BadRequest("User does not exist");

            var userWithUsername = await _dbService.RetrieveUserByUsername(username);

            if (userWithUsername != null)
                return BadRequest("Username already exists");

            existingUser.UserName = username;

            await _dbService.UpdateUser(existingUser);

            return Ok(existingUser);
        }

        [HttpPost("updatepassword")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdatePassword([FromQuery] int id, [FromQuery] string password)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
                return BadRequest("User does not exist");

            existingUser.Password = password;

            await _dbService.UpdateUser(existingUser);

            return Ok(existingUser);
        }

        [HttpPost("updateuserinfo")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateUserInfo([FromQuery] int id, [FromBody] Models.UserInfoDto updatedUserInfo)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
                return BadRequest("User does not exist");

            if(updatedUserInfo.Email != existingUser.Email)
            {
                var userWithEmail = await _dbService.RetrieveUserByEmail(updatedUserInfo.Email);

                if (userWithEmail != null)
                    return BadRequest("Email already exists");
            }

            existingUser.FirstName = updatedUserInfo.FirstName;
            existingUser.LastName = updatedUserInfo.LastName;
            existingUser.DateOfBirth = updatedUserInfo.DateOfBirth;
            existingUser.Email = updatedUserInfo.Email;

            await _dbService.UpdateUser(existingUser);

            return Ok(existingUser);
        }
    }

    [Route("api/userprofile")]
    [ApiController]
    public class UserProfileController(IDbService dbService, ITransactionService transactionService) : ControllerBase
    {
        private readonly IDbService _dbService = dbService;

        [HttpPost("getuserprofile")]
        [ProducesResponseType(200, Type = typeof(Entities.UserProfile))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUserProfile([FromQuery] int id)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
                return BadRequest("User does not exist");

            var userProfiles = await _dbService.RetrieveUserProfile(id);

            if (userProfiles == null)
                return NoContent();

            if(userProfiles.User.ID != id)
                return BadRequest("User profile does not exist");

            return Ok(userProfiles);
        }

        //Will be depracated since the balance shouldn't be updated directly
        [HttpPost("updateuserprofilebalance")]
        [ProducesResponseType(200, Type = typeof(Entities.UserProfile))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateUserProfileBalance([FromQuery] int id, [FromQuery] decimal balance)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
                return BadRequest("User does not exist");

            var existingUserProfile = await _dbService.RetrieveUserProfile(id);

            if (existingUserProfile == null)
                return BadRequest("User profile does not exist");

            existingUserProfile.Balance = balance;

            await _dbService.UpdateUserProfile(existingUserProfile);

            return Ok(existingUserProfile);
        }

        //Need to add in summary that the id is the user id
        [HttpPost("buy")]
        [ProducesResponseType(204, Type = typeof(Entities.UserTransaction))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Buy([FromQuery] int id, [FromBody] Models.BuyStockDto buyStock)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
                return BadRequest("User does not exist");

            var existingUserProfile = await _dbService.RetrieveUserProfile(id);

            if (existingUserProfile == null)
                return BadRequest("User profile does not exist");

            var company = await _dbService.RetrieveCompanyBySymbol(buyStock.Symbol);

            if (company == null)
                return BadRequest("Company does not exist");

            decimal currentStockPrice = new decimal();

            if(buyStock.DeviatedPrice > 0)
                currentStockPrice = buyStock.DeviatedPrice;
            else
                currentStockPrice = await _dbService.GetCurrentStockPriceOfCompany(buyStock.Symbol);

            var stockAmount = buyStock.Value / currentStockPrice;

            if (existingUserProfile.Balance < buyStock.Value)
                return BadRequest("Insufficient funds");

            var userTransaction = new Entities.UserTransaction
            {
                Company = company,
                Quantity = stockAmount,
                TransactionType = Entities.Enum.TransactionType.Buy,
                TransactionStatus = Entities.Enum.TransactionStatus.Pending,
                TransactionValue = buyStock.Value,
                StockValue = currentStockPrice,
                TimeOfTransaction = DateTime.Now
            };

            existingUserProfile.UserTransactions.Add(userTransaction);

            await _dbService.UpdateUserProfile(existingUserProfile);

            var executedResponse = await transactionService.ExecuteTransaction(existingUserProfile, userTransaction);

            return Ok(executedResponse);
        }

        //Need to add in summary that the id is the user id
        [HttpPost("sell")]
        [ProducesResponseType(204, Type = typeof(Entities.UserTransaction))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Sell([FromQuery] int id, [FromBody] Models.SellStockDto sellStock)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
                return BadRequest("User does not exist");

            var existingUserProfile = await _dbService.RetrieveUserProfile(id);

            if (existingUserProfile == null)
                return BadRequest("User profile does not exist");

            var company = await _dbService.RetrieveCompanyBySymbol(sellStock.Symbol);

            if (company == null)
                return BadRequest("Company does not exist");

            decimal currentStockPrice = new decimal();

            if(sellStock.DeviatedPrice > 0)
                currentStockPrice = sellStock.DeviatedPrice;
            else
                currentStockPrice = await _dbService.GetCurrentStockPriceOfCompany(sellStock.Symbol);

            var stockAmount = sellStock.Value / currentStockPrice;

            var userTransaction = new Entities.UserTransaction
            {
                Company = company,
                Quantity = stockAmount,
                TransactionType = Entities.Enum.TransactionType.Sell,
                TransactionStatus = Entities.Enum.TransactionStatus.Pending,
                TransactionValue = sellStock.Value,
                StockValue = currentStockPrice,
                TimeOfTransaction = DateTime.Now
            };

            existingUserProfile.UserTransactions.Add(userTransaction);

            await _dbService.UpdateUserProfile(existingUserProfile);

            var executedResponse = await transactionService.ExecuteTransaction(existingUserProfile, userTransaction);

            return Ok(executedResponse);
        }

        [HttpPost("canceltransaction")]
        [ProducesResponseType(202, Type = typeof(Entities.UserTransaction))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CancelTransaction([FromBody] Models.CancelTransactionDto cancelTransaction)
        {
            var existingTransaction = await _dbService.RetrieveUserTransaction(cancelTransaction.TransactionID);

            if (existingTransaction == null)
                return BadRequest("User transaction does not exist");

            var executedResponse = await transactionService.CancelTransaction(existingTransaction);

            return Accepted(executedResponse);
        }


        ///<summary>
        ///Resets the user profile by clearing all transactions and portfolio stocks and setting the balance to the specified difficulty level
        ///id: The user id
        ///difficulty: The difficulty level to set the balance to (easy, medium, hard)
        ///</summary>
        [HttpPost("resetprofile")]
        [ProducesResponseType(200, Type = typeof(Entities.UserProfile))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetProfile([FromQuery] int id, [FromQuery] string difficulty)
        {
            var existingUser = await _dbService.RetrieveUser(id);

            if (existingUser == null)
            return BadRequest("User does not exist");

            var existingUserProfile = await _dbService.RetrieveUserProfile(id);

            if (existingUserProfile == null)
            return BadRequest("User profile does not exist");

            existingUserProfile.UserTransactions.Clear();
            existingUserProfile.UserPortfolioStocks.Clear();

            switch (difficulty.ToLower())
            {
            case "easy":
                existingUserProfile.Balance = 10000;
                break;
            case "medium":
                existingUserProfile.Balance = 5000;
                break;
            case "hard":
                existingUserProfile.Balance = 1000;
                break;
            default:
                return BadRequest("Invalid difficulty level");
            }

            await _dbService.UpdateUserProfile(existingUserProfile);

            return Ok(existingUserProfile);
        }
    }
}