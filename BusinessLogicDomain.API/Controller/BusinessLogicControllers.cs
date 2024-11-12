using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Microsoft.AspNetCore.Mvc;

namespace BusinessLogicDomain.API.Controller
{
    [Route("api/marketdata")]
    [ApiController]
    public class MarketDataController(MarketDataDomainClient marketDataClient, IDbService dbService) : ControllerBase
    {
        private readonly MarketDataDomainClient _marketDataClient = marketDataClient;
        private readonly IDbService _dbService = dbService;

        [HttpGet("getallcompanies")]
        [ProducesResponseType(200, Type = typeof(List<Entities.Company>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _dbService.RetrieveInitializedCompanies();

            if (companies == null)
                return NoContent();

            return Ok(companies);
        }

        [HttpGet("marketdata/getcompanylivepricedistinct")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Entities.LivePriceDistinct>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCompanyLivePriceDistinct([FromQuery] string symbols)
        {
            if (string.IsNullOrEmpty(symbols))
                return BadRequest("Symbols parameter is required.");

            var symbolList = symbols.Split(',').Select(s => s.Trim()).ToList();

            var prices = new List<Entities.LivePriceDistinct>();

            foreach (var symbol in symbolList)
            {
                var company = await _dbService.RetrieveCompanyBySymbol(symbol);
                if (company == null)
                    continue;

                var price = await _dbService.GetCompanyLivePriceDistinct(symbol);
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
            var company = await _dbService.RetrieveCompanyBySymbol(symbol);

            if (company == null)
                return NoContent();

            var priceHistory = await _dbService.GetCompanyLivePriceDaily(symbol);

            if (priceHistory == null)
                return NoContent();

            return Ok(priceHistory);
        }

        [HttpGet("marketdata/getcompanypricehistory")]
        [ProducesResponseType(200, Type = typeof(List<Entities.PriceHistory>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCompanyPriceHistory([FromQuery] string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var company = await _dbService.RetrieveCompanyBySymbol(symbol);

            if (company == null)
                return NoContent();

            var priceHistory = await _dbService.GetCompanyPriceHistory(symbol, startDate, endDate);

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
        public async Task<IActionResult> Register([FromBody] Models.UserRegisterDTO newUser)
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
        public async Task<IActionResult> Login([FromBody] Models.UserLoginDTO userLogin)
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
        public async Task<IActionResult> UpdateUserInfo([FromQuery] int id, [FromBody] Models.UserInfoDTO updatedUserInfo)
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
    public class UserProfileController(IDbService dbService) : ControllerBase
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

            return Ok(userProfiles);
        }

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
    }
}