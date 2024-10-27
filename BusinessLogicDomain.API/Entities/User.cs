using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessLogicDomain.API.Entities
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        [Required]
        public required DateTime DateOfBirth { get; set; }
        [Required]
        public required string Address { get; set; }
        //Deleted user profile

    }
}