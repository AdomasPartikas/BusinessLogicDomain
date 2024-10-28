using BusinessLogicDomain.API.Services;
using BusinessLogicDomain.MarketDataDomainAPIClient;
using Microsoft.AspNetCore.Mvc;

namespace BusinessLogicDomain.API.Controller
{
    [Route("api")]
    [ApiController]
    public class BusinessLogicController(MarketDataDomainClient marketDataClient, IDbService dbService) : ControllerBase
    {
        private readonly MarketDataDomainClient _marketDataClient = marketDataClient;
        private readonly IDbService _dbService = dbService;
        
        [HttpGet("marketdata/getallcompanies")]
        [ProducesResponseType(200, Type = typeof(List<Entities.Company>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _dbService.RetrieveInitializedCompanies();

            if(companies == null)
                return NoContent();

            return Ok(companies);
        }

        [HttpGet("marketdata/getcompanylivepricedistinct/{symbol}")]
        [ProducesResponseType(200, Type = typeof(Entities.LivePriceDistinct))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCompanyLivePriceDistinct(string symbol)
        {
            var company = await _dbService.RetrieveCompanyBySymbol(symbol);

            if(company == null)
                return NoContent();

            var price = await _dbService.GetCompanyLivePriceDistinct(symbol);

            if(price == null)
                return NoContent();

            return Ok(price);
        }

        [HttpGet("marketdata/getcompanylivepricedaily/{symbol}")]
        [ProducesResponseType(200, Type = typeof(List<Entities.LivePriceDaily>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCompanyLivePriceDaily(string symbol)
        {
            var company = await _dbService.RetrieveCompanyBySymbol(symbol);

            if(company == null)
                return NoContent();

            var priceHistory = await _dbService.GetCompanyLivePriceDaily(symbol);

            if(priceHistory == null)
                return NoContent();

            return Ok(priceHistory);
        }

        [HttpGet("marketdata/getcompanypricehistory/{symbol}")]
        [ProducesResponseType(200, Type = typeof(List<Entities.PriceHistory>))]
        [ProducesResponseType(204)]
        public async Task<IActionResult> GetCompanyPriceHistory(string symbol, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var company = await _dbService.RetrieveCompanyBySymbol(symbol);

            if(company == null)
                return NoContent();

            var priceHistory = await _dbService.GetCompanyPriceHistory(symbol, startDate, endDate);

            if(priceHistory == null || priceHistory.Count == 0)
                return NoContent();

            return Ok(priceHistory);
        }

        [HttpPost("user/createuser")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateUser([FromBody] Models.UserDTO newUser)
        {
            var existingUser = await _dbService.RetrieveUser(newUser.UserName);

            if(existingUser != null)
                return BadRequest("User already exists");

            var user = await _dbService.CreateUser(newUser);

            return Ok(user);
        }

        [HttpPost("user/login")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login([FromBody] Models.UserLoginDTO userLogin)
        {
            var existingUser = await _dbService.RetrieveUser(userLogin.UserName);

            if(existingUser == null)
                return BadRequest("User does not exist");

            if(existingUser.Password != userLogin.Password)
                return BadRequest("Incorrect password");

            return Ok(existingUser);
        }

        [HttpPost("user/updateusercredentials")]
        [ProducesResponseType(200, Type = typeof(Entities.User))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateUserCredentials([FromBody] Models.UserDTO updatedUser)
        {
            var existingUser = await _dbService.RetrieveUser(updatedUser.UserName);

            if(existingUser == null)
                return BadRequest("User does not exist");

            existingUser.Password = updatedUser.Password;
            existingUser.FirstName = updatedUser.FirstName;
            existingUser.LastName = updatedUser.LastName;
            existingUser.DateOfBirth = updatedUser.DateOfBirth;
            existingUser.Address = updatedUser.Address;

            await _dbService.UpdateUser(existingUser);

            return Ok(existingUser);
        }

        [HttpPost("userprofile/getuserprofile")]
        [ProducesResponseType(200, Type = typeof(Entities.UserProfile))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUserProfile([FromQuery] string userName)
        {
            var existingUser = await _dbService.RetrieveUser(userName);

            if(existingUser == null)
                return BadRequest("User does not exist");

            var userProfiles = await _dbService.RetrieveUserProfile(userName);

            if(userProfiles == null)
                return NoContent();

            return Ok(userProfiles);
        }

        [HttpPost("userprofile/updateuserprofilebalance")]
        [ProducesResponseType(200, Type = typeof(Entities.UserProfile))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateUserProfileBalance([FromQuery] string userName, [FromQuery] decimal balance)
        {
            var existingUser = await _dbService.RetrieveUser(userName);

            if(existingUser == null)
                return BadRequest("User does not exist");

            var existingUserProfile = await _dbService.RetrieveUserProfile(userName);

            if(existingUserProfile == null)
                return BadRequest("User profile does not exist");

            existingUserProfile.Balance = balance;

            await _dbService.UpdateUserProfile(existingUserProfile);

            return Ok(existingUserProfile);
        }
    }
}