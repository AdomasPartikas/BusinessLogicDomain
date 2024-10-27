namespace BusinessLogicDomain.API.Models
{
    public class UserDTO
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public required string Address { get; set; }
        public required decimal Balance { get; set; }
    }

    public class UserLoginDTO
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}