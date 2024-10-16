namespace BusinessLogicDomain.API.Models;

public class UserProfile
{
    public required int ID { get; set; }
    public required User User { get; set; }
    public required string Balance { get; set; }
}