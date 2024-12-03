using BusinessLogicDomain.API.Entities.Enum;

namespace BusinessLogicDomain.API.Models
{
    public class UserRegisterDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required string Email { get; set; }
        public required decimal Balance { get; set; }
        public required SimulationLevel SimulationLevel { get; set; }
    }

    public class UserLoginDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }

    public class UserInfoDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required string Email { get; set; }
    }
}