using System.ComponentModel.DataAnnotations;

namespace BusinessLogicDomain.API.Models;

public class UserProfile
{
    public required int ID { get; set; }
    [Required]
    public required User User { get; set; }
    [Required]
    public required string Balance { get; set; }
}