using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;
public class User
{
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
}